#! /bin/bash
VPNIF="tun0"
TABLE="42"
MARK="0x42"
GATEWAYIP=`ifconfig $VPNIF | egrep -o '([0-9]{1,3}\.){3}[0-9]{1,3}' | egrep -v '255|(127\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})' | tail -n1`

if [[ `ip rule list | grep -c $MARK` == 0 ]]; then
#ip rule add from all fwmark $MARK lookup $TABLE
ip rule add fwmark $MARK $TABLE
fi
ip route replace default via $GATEWAYIP table $TABLE
ip route flush cache

#echo 2 > /proc/sys/net/ipv4/conf/tun0/rp_filter
