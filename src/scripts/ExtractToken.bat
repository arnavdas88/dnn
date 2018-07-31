setlocal

set SN="C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7.2 Tools\x64\sn.exe"

%SN% -p %1 token.snk
%SN% -tp token.snk