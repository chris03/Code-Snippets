
namespace Chris03.CodeSnippets.EntityFrameworkAuditLog
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Threading;

    public class AuditLog
    {
        protected AuditLog()
        {
            this.Timestamp = DateTimeOffset.Now;
            this.Username = GetCurrentUsername();
        }

        public int Id { get; protected set; }

        public virtual List<AuditLogItem> Items { get; set; }

        public DateTimeOffset Timestamp { get; private set; }

        public string Username { get; private set; }

        public class AuditLogItem
        {
            private readonly object entity;

            private string newValue;

            private string originalValue;

            public AuditLogItem(string state, string tableName, object entity)
                : this(state, tableName, "Id", string.Empty, string.Empty)
            {
                this.entity = entity;
            }

            public AuditLogItem(string state, string tableName, string columnName, string originalValue, string newValue)
            {
                this.State = state;
                this.TableName = tableName;
                this.ColumnName = columnName;
                this.OriginalValue = originalValue;
                this.NewValue = newValue;
            }

            protected AuditLogItem()
            {
            }

            public int Id { get; protected set; }

            public string State { get; protected set; }

            public string TableName { get; protected set; }

            public string ColumnName { get; protected set; }

            public string OriginalValue
            {
                get
                {
                    return this.entity == null ? this.originalValue : this.GetEntityId();
                }
                protected set
                {
                    this.originalValue = value;
                }
            }

            public string NewValue
            {
                get
                {
                    return this.entity == null ? this.newValue : this.GetEntityId();
                }
                protected set
                {
                    this.newValue = value;
                }
            }

            protected string GetEntityId()
            {
                var id = ((dynamic)this.entity).Id;

                return id == null ? "?" : Convert.ToString(id);
            }
        }

        public static AuditLog GetAuditLog(DbContext context, params string[] ignoredEntities)
        {
            var auditLog = new AuditLog { Items = new List<AuditLogItem>() };

            var changeTrack = context.ChangeTracker.Entries().Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified);

            foreach (var entry in changeTrack)
            {
                if (entry.Entity != null)
                {
                    var entityName = ObjectContext.GetObjectType(entry.Entity.GetType()).Name;
                    var state = entry.State.ToString();

                    if (IsIgnored(entry.Entity, entityName, ignoredEntities))
                    {
                        // Do not log
                        continue;
                    }

                    switch (entry.State)
                    {
                        // Modified
                        case EntityState.Modified:
                            // Log id and changed values
                            auditLog.Items.Add(new AuditLogItem(state, entityName, entry.Entity));
                            foreach (var propertyName in entry.OriginalValues.PropertyNames)
                            {
                                object currentValue = entry.CurrentValues[propertyName];
                                object originalValue = entry.OriginalValues[propertyName];

                                CheckChanges(auditLog.Items, originalValue, currentValue, entityName, state, propertyName);
                            }
                            break;

                        // Added
                        case EntityState.Added:
                            // Log only id
                            auditLog.Items.Add(new AuditLogItem(state, entityName, entry.Entity));
                            break;

                        // Deleted
                        case EntityState.Deleted:
                            // Log only id
                            auditLog.Items.Add(new AuditLogItem(state, entityName, entry.Entity));
                            break;
                    }
                }
            }
            return auditLog;
        }

        private static void CheckChanges(ICollection<AuditLogItem> items, object originalValue, object currentValue, string entityName, string state, string propertyName)
        {
            var currentComplex = currentValue as DbPropertyValues;
            var originalComplex = originalValue as DbPropertyValues;

            if (currentComplex != null && originalComplex != null)
            {
                foreach (var name in originalComplex.PropertyNames)
                {
                    CheckChanges(items, originalComplex[name], currentComplex[name], entityName, state, propertyName + "." + name);
                }
            }
            else
            {
                if (!Equals(currentValue, originalValue))
                {
                    items.Add(new AuditLogItem(state, entityName, propertyName, Convert.ToString(originalValue), Convert.ToString(currentValue)));
                }
            }
        }

        private static bool IsIgnored(object entity, string entityName, params string[] ignoredEntities)
        {
            return entity is AuditLog || entity is AuditLogItem || (ignoredEntities != null && ignoredEntities.Contains(entityName));
        }

        private static string GetCurrentUsername()
        {
            var principal = Thread.CurrentPrincipal;

            return principal != null && principal.Identity != null && principal.Identity.IsAuthenticated ? principal.Identity.Name : string.Empty;
        }
    }
}
