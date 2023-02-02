using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EntityGeneratorWindows.Common
{
    public class WinCapHelper
    {
        private WinCapHelper() { }
        private static WinCapHelper instance;
        public static WinCapHelper Get()
        {
            return instance ?? new WinCapHelper();
        }

        public Action<string> _logAction;
        public string filter;

        public void ListenBegin()
        {
            Task.Factory.StartNew(() => {
                foreach (PcapDevice device in CaptureDeviceList.Instance)
                {
                    device.OnPacketArrival += Device_OnPacketArrival;
                    device.Open();
                    device.Capture(500);
                }
            });
        }

        public void ListenEnd()
        {
            foreach (PcapDevice device in CaptureDeviceList.Instance)
            {
                if (device.Opened)
                {
                    Task.Delay(500);
                    device.StopCapture();
                }
            }
        }

        private void PrintPacket(out string value, Packet p)
        {
            value = "";
            if (p is not null)
            {
                if (!string.IsNullOrWhiteSpace(filter) && !p.ToString().Contains(filter))
                {
                    return;
                }
                else
                {
                    value += "\r\n" + p.ToString() + "\r\n";
                    value += p.PrintHex() + "\r\n";
                }
            }
        }

        private void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            //解析出基本包  
            var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
            //协议类别  
            //var dlPacket = PacketDotNet.DataLinkPacket.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            //var ethernetPacket = PacketDotNet.EthernetPacket.GetEncapsulated(packet);
            //var internetLinkPacket = PacketDotNet.InternetLinkLayerPacket.Parse(packet.BytesHighPerformance.Bytes);
            //var internetPacket = PacketDotNet.InternetPacket.Parse(packet.BytesHighPerformance.Bytes);
            //var sessionPacket = PacketDotNet.SessionPacket.Parse(packet.BytesHighPerformance.Bytes);
            //var appPacket = PacketDotNet.ApplicationPacket.Parse(packet.BytesHighPerformance.Bytes);
            //var pppoePacket = PacketDotNet.PPPoEPacket.Parse(packet.BytesHighPerformance.Bytes);
            //var arpPacket = PacketDotNet.ARPPacket.GetEncapsulated(packet);
            //var ipPacket = PacketDotNet.IpPacket.GetEncapsulated(packet); //ip包  
            //var udpPacket = PacketDotNet.UdpPacket.GetEncapsulated(packet);
            //var tcpPacket = PacketDotNet.TcpPacket.GetEncapsulated(packet);
            PrintPacket(out string ret, packet);
            //ParsePacket(ref ret, ethernetPacket);  
            //ParsePacket(ref ret, internetLinkPacket);  
            //ParsePacket(ref ret, internetPacket);  
            //ParsePacket(ref ret, sessionPacket);  
            //ParsePacket(ref ret, appPacket);  
            //ParsePacket(ref ret, pppoePacket);  
            //ParsePacket(ref ret, arpPacket);  
            //ParsePacket(ref ret, ipPacket);  
            //ParsePacket(ref ret, udpPacket);  
            //ParsePacket(ref ret, tcpPacket);  
            if (!string.IsNullOrEmpty(ret))
            {
                string rlt = "\r\n时间 : " +
                    DateTime.Now.ToLongTimeString() +
                    "\r\n数据包: \r\n" + ret;
                _logAction(rlt);
            }
        }
    }
}
