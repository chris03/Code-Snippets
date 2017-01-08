#! /bin/bash

export INTERFACE="tun0"
export VPNUSER="debian-transmission"

# Clear rules
iptables -F -t nat
iptables -F -t mangle
iptables -F -t filter

iptables -P INPUT DROP

# Loopback
iptables -A INPUT -i lo -j ACCEPT

# Accept established connections
iptables -A INPUT -m state --state ESTABLISHED,RELATED -j ACCEPT
iptables -A OUTPUT  -m state --state ESTABLISHED,RELATED -j ACCEPT
iptables -A FORWARD -m state --state ESTABLISHED,RELATED -j ACCEPT

# Ping
iptables -A INPUT -p ICMP --icmp-type 8 -j ACCEPT
iptables -A INPUT -p ICMP --icmp-type 11 -j ACCEPT

# SSH & HTTP
iptables -A INPUT -i eth0 -p tcp -m tcp --dport 80 -j ACCEPT
iptables -A INPUT -i eth0 -p tcp -m tcp --dport 31337 -j ACCEPT

# Mark packets
iptables -t mangle -A OUTPUT -m owner --uid-owner $VPNUSER -j MARK --set-xmark 0x42
#iptables -t mangle -A OUTPUT -m owner --uid-owner $VPNUSER -j CONNMARK --save-mark
#iptables -t mangle -A PREROUTING -j CONNMARK --restore-mark
#iptables -t mangle -A OUTPUT -j CONNMARK --restore-mark

# Allow torrent
iptables -A INPUT -i $INTERFACE -p tcp --dport 59560 -j ACCEPT

#iptables -t nat -A OUTPUT -p udp --dport 53 -j DNAT --to 8.8.8.8:53 -o tun0
#iptables -t nat -A OUTPUT -p tcp --dport 53 -j DNAT --to 8.8.8.8:53 -o tun0

# Allow lo and $INTERFACE, Disallow eth0
#iptables -A OUTPUT -o eth0 -m owner --uid-owner $VPNUSER -j REJECT

# all packets on $INTERFACE needs to be masqueraded
#iptables -t nat -A POSTROUTING -o $INTERFACE -j MASQUERADE

#iptables -F LOGGING
#iptables -N LOGGING
#iptables -A OUTPUT -j LOGGING
#iptables -A OUTPUT -o eth0 -m owner --uid-owner $VPNUSER -j LOGGING
#iptables -A LOGGING -m limit --limit 2/min -j LOG --log-prefix "IPTables-Dropped: " --log-level 4
#iptables -A LOGGING -j DROP
