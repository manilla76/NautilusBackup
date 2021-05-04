using System;
using System.Linq;
using Thermo.Datapool;

namespace ThermoDpSQLReader.Model
{
    static class DataManager
    {
        public static Datapool.ITagInfo TagInfo;
        public static Datapool.ITagInfo TerminateTag;
        private static IDataService dataService = new DataService();
        public static event EventHandler OnShutdown;

        public static void Initialize()
        {
            TerminateTag = Datapool.DatapoolSvr.CreateTagInfo("SYSTEM", "System terminate", Datapool.dpTypes.BOOL);
            TagInfo = Datapool.DatapoolSvr.CreateTagInfo(Properties.Settings.Default.ReadGroup, Properties.Settings.Default.ReadTag, GetDataType());
            TagInfo.UpdateValueEvent += TagInfo_UpdateValueEvent;
            TerminateTag.UpdateValueEvent += TerminateTag_UpdateValueEvent;
        }

        private static Datapool.dpTypes GetDataType()
        {
            Datapool.DPGroupTagList list = Datapool.DatapoolSvr.ReadTagNames();
            Datapool.DPGroupTagName tag = list.m_tagNames.FirstOrDefault(
                x => string.Compare(x.m_group, Properties.Settings.Default.ReadGroup, true) == 0
                && string.Compare(x.m_tag, Properties.Settings.Default.ReadTag, true) == 0);
            if (tag == null)
            {
                Datapool.DatapoolSvr.Create(Properties.Settings.Default.ReadGroup, Properties.Settings.Default.ReadTag, Datapool.dpTypes.STRING);
                return Datapool.dpTypes.STRING;
            }
            return tag.m_type;

        }

        private static void TerminateTag_UpdateValueEvent(Datapool.ITagInfo e)
        {
            if (e.AsBoolean == true)
            {
                OnShutdown(e, EventArgs.Empty);
            }
        }

        public static void TagInfoUpdate()
        {
            TagInfo.Dispose();
            TagInfo = Datapool.DatapoolSvr.CreateTagInfo(Properties.Settings.Default.ReadGroup, Properties.Settings.Default.ReadTag, GetDataType());
            TagInfo.UpdateValueEvent += TagInfo_UpdateValueEvent;
        }

        private static void TagInfo_UpdateValueEvent(Datapool.ITagInfo e)
        {
            Datapool.DatapoolSvr.Create(Properties.Settings.Default.WriteGroup, Properties.Settings.Default.WriteTag, Datapool.dpTypes.STRING);
            Datapool.DatapoolSvr.Write(Properties.Settings.Default.WriteGroup, Properties.Settings.Default.WriteTag, dataService.ReadSQL(e.AsString));
        }
    }
}
