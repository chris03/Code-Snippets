#!/bin/sh
iface="tun0"
service="transmission-daemon"
config_file="/etc/transmission-daemon/settings.json"
# test if service is currently running
if ps ax | grep -v grep | grep $service >/dev/null
then
    configured_ip=$(cat $config_file | egrep -o '\"bind-address-ipv4\": \"[^ ]*' | cut -d'"' -f4)
    echo bind-address-ipv4: $configured_ip
    # test if ip has changed and is no longer available
    ping -i 1 -w 2 $configured_ip >/dev/null
    if [ $? -ne 0 ]; then
        echo IP unavailable, stop and reconfigure transmission...
        invoke-rc.d $service stop
# test if VPN went down completely
        ifconfig $iface >/dev/null
        if [ $? -ne 0 ]; then
            echo uups...$iface not available, exiting without restarting transmission...
            exit 1
        fi
# configure transmission with new ip address
        sed -i "s/\"bind-address-ipv4\":.*\$/\"bind-address-ipv4\": \"$(ifconfig $iface | egrep -o 'addr:[^ ]* ' | cut -d':' -f2 | sed 's/ //')\",/" $config_file
        service $service start
        echo transmission started.

        configured_ip=$(cat $config_file | egrep -o '\"bind-address-ipv4\": \"[^ ]*' | cut -d'"' -f4)
        echo bind-address-ipv4: $configured_ip
    else
       echo Ping ok
    fi
else
  echo Transmission is not running...
fi
