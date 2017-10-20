
        public class PortScaner
        {
            public delegate void OpenPortFoundDelegate(int port, string protocolType);

            public delegate void ScanCopliteDelegate(IEnumerable<PortScaner.ScannedPort> scannedPorts);

            public class ScannedPort
            {
                public int Port
                {
                    get;
                    private set;
                }

                public bool IsOpen
                {
                    get;
                    private set;
                }

                public ProtocolType ProtocolType
                {
                    get;
                    private set;
                }

                public ScannedPort(int port, bool isOpen, ProtocolType protocolType)
                {
                    this.Port = port;
                    this.ProtocolType = protocolType;
                    this.IsOpen = isOpen;
                }
            }

            public static ScannedPort Scan(IPAddress target, int port, int timeout=10)
            {
                ScannedPort result = null;
                using (TcpClient tcpClient = new TcpClient())
                {
                    IAsyncResult asyncResult = tcpClient.BeginConnect(target, port, null, null);
                    result = new ScannedPort(port, asyncResult.AsyncWaitHandle.WaitOne(timeout, false), tcpClient.Client.ProtocolType);
                }
                return result;
            }

            public static void Scan(IPAddress target, ushort startPort, ushort endPort, OpenPortFoundDelegate portFoundCallBack = null, ScanCopliteDelegate scanCompliteCallBack = null, int timeout=10)
            {
                int num = endPort - startPort + 1;
                ScannedPort[] portScannerArray = new ScannedPort[num];
                Parallel.For(0, endPort, delegate (int i)
                {
                    portScannerArray[i] = Scan(target, i + startPort, timeout);
                    bool flag2 = portFoundCallBack != null && portScannerArray[i].IsOpen;
                    if (flag2)
                    {
                        portFoundCallBack.BeginInvoke(portScannerArray[i].Port, portScannerArray[i].ProtocolType.ToString(), null, null);
                    }
                });
                bool flag = scanCompliteCallBack != null;
                if (flag)
                {
                    scanCompliteCallBack.BeginInvoke(portScannerArray, null, null);
                }
            }
        }