using MahApps.Metro.Controls.Dialogs;
using System;

namespace EntityGeneratorWindows.Common
{
    public class TheEvent
    {
        public static EventHandler<InfoEventArgs> DBInfoChangedEvent;

        public static void SendDBInfoChangedEvent(object sender, InfoEventArgs args)
        {
            DBInfoChangedEvent?.Invoke(sender, args);
        }

        public static EventHandler<InfoEventArgs> JSONInfoChangedEvent;

        public static void SendJSONInfoChangedEvent(object sender, InfoEventArgs args)
        {
            JSONInfoChangedEvent?.Invoke(sender, args);
        }

        public static EventHandler<MessageEventArgs> ShowMessageEvent;

        public static void ShowMessageBox(string msg, string title)
        {
            ShowMessageEvent?.Invoke(null, new MessageEventArgs(msg, title));
        }

        public static EventHandler<MessageEventArgs> ShowProgressEvent;

        public static void ShowProgressBox(string msg, string title)
        {
            ShowProgressEvent?.Invoke(null, new MessageEventArgs(msg, title));
        }
    }

    public class InfoEventArgs : EventArgs
    {
        public string info { get; }

        public InfoEventArgs(string info)
        {
            this.info = info;
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public string msg { get; }
        public string title { get; }

        public MessageDialogStyle style { get; set; }

        public MessageEventArgs(string msg, string title, MessageDialogStyle style = MessageDialogStyle.Affirmative)
        {
            this.msg = msg;
            this.title = title;
            this.style = style;
        }
    }
}
