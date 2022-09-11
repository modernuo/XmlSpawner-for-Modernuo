using System;
using Server.Items;
using System.Collections;
using Server.Gumps;

namespace Server.Engines.XmlSpawner2;

public class XmlQuestPoints : XmlAttachment
{
    private int m_Points;
    private int m_Completed;
    private int m_Credits;

    private ArrayList m_QuestList = new();

    private DateTime m_WhenRanked;
    private int m_Rank;
    private int m_DeltaRank;

    public string guildFilter;
    public string nameFilter;


    public ArrayList QuestList { get => m_QuestList;
        set => m_QuestList = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int Rank { get => m_Rank;
        set => m_Rank = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int DeltaRank { get => m_DeltaRank;
        set => m_DeltaRank = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public DateTime WhenRanked { get => m_WhenRanked;
        set => m_WhenRanked = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int Points { get => m_Points;
        set => m_Points = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int Credits { get => m_Credits;
        set => m_Credits = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int QuestsCompleted { get => m_Completed;
        set => m_Completed = value;
    }

    public class QuestEntry
    {
        public Mobile      Quester;
        public string Name;
        public DateTime    WhenCompleted;
        public DateTime    WhenStarted;
        public int Difficulty;
        public bool PartyEnabled;
        public int TimesCompleted = 1;

        public QuestEntry()
        {
        }

        public QuestEntry(Mobile m, IXmlQuest quest)
        {
            Quester = m;
            if (quest != null)
            {
                WhenStarted = quest.TimeCreated;
                WhenCompleted = DateTime.Now;
                Difficulty = quest.Difficulty;
                Name = quest.Name;
            }
        }

        public virtual void Serialize(IGenericWriter writer)
        {

            writer.Write(0); // version

            writer.Write(Quester);
            writer.Write(Name);
            writer.Write(WhenCompleted);
            writer.Write(WhenStarted);
            writer.Write(Difficulty);
            writer.Write(TimesCompleted);
            writer.Write(PartyEnabled);


        }

        public virtual void Deserialize(IGenericReader reader)
        {

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        Quester = reader.ReadEntity<Mobile>();
                        Name = reader.ReadString();
                        WhenCompleted = reader.ReadDateTime();
                        WhenStarted = reader.ReadDateTime();
                        Difficulty = reader.ReadInt();
                        TimesCompleted = reader.ReadInt();
                        PartyEnabled = reader.ReadBool();
                        break;
                    }
            }

        }

        public static void AddQuestEntry(Mobile m, IXmlQuest quest)
        {
            if (m == null || quest == null)
            {
                return;
            }

            // get the XmlQuestPoints attachment from the mobile
            XmlQuestPoints p = (XmlQuestPoints)XmlAttach.FindAttachment(m, typeof(XmlQuestPoints));

            if (p == null)
            {
                return;
            }

            // look through the list of quests and see if it is one that has already been done
            if (p.QuestList == null)
            {
                p.QuestList = new ArrayList();
            }

            bool found = false;
            foreach(QuestEntry e in p.QuestList)
            {
                if (e.Name == quest.Name)
                {
                    // found a match, so just change the number and dates
                    e.TimesCompleted++;
                    e.WhenStarted = quest.TimeCreated;
                    e.WhenCompleted = DateTime.Now;
                    // and update the difficulty and party status
                    e.Difficulty = quest.Difficulty;
                    e.PartyEnabled = quest.PartyEnabled;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // add a new entry
                p.QuestList.Add(new QuestEntry(m, quest));

            }
        }
    }


    public XmlQuestPoints(ASerial serial) : base(serial)
    {
    }

    [Attachable]
    public XmlQuestPoints()
    {
    }

    public static new void Initialize()
    {
        CommandSystem.Register("QuestPoints", AccessLevel.Player, CheckQuestPoints_OnCommand);

        CommandSystem.Register("QuestLog", AccessLevel.Player, QuestLog_OnCommand);

    }

    [Usage("QuestPoints")]
    [Description("Displays the players quest points and ranking")]
    public static void CheckQuestPoints_OnCommand(CommandEventArgs e)
    {
        if (e == null || e.Mobile == null)
        {
            return;
        }

        string msg = null;

        XmlQuestPoints p = (XmlQuestPoints)XmlAttach.FindAttachment(e.Mobile, typeof(XmlQuestPoints));
        if (p != null)
        {
            msg = p.OnIdentify(e.Mobile);
        }

        if (msg != null)
        {
            e.Mobile.SendMessage(msg);
        }
    }



    [Usage("QuestLog")]
    [Description("Displays players quest history")]
    public static void QuestLog_OnCommand(CommandEventArgs e)
    {
        if (e == null || e.Mobile == null)
        {
            return;
        }

        e.Mobile.CloseGump<QuestLogGump>();
        e.Mobile.SendGump(new QuestLogGump(e.Mobile));
    }


    public static void GiveQuestPoints(Mobile from, IXmlQuest quest)
    {
        if (from == null || quest == null)
        {
            return;
        }

        // find the XmlQuestPoints attachment

        XmlQuestPoints p = (XmlQuestPoints)XmlAttach.FindAttachment(from, typeof(XmlQuestPoints));

        // if doesnt have one yet, then add it
        if (p == null)
        {
            p = new XmlQuestPoints();
            XmlAttach.AttachTo(from, p);
        }

        // if you wanted to scale the points given based on party size, karma, fame, etc.
        // this would be the place to do it
        int points = quest.Difficulty;

        // update the questpoints attachment information
        p.Points += points;
        p.Credits += points;
        p.QuestsCompleted++;

        if (from != null)
        {
            from.SendMessage("You have received {0} quest points!",points);
        }

        // add the completed quest to the quest list
        QuestEntry.AddQuestEntry(from, quest);

        // update the overall ranking list
        XmlQuestLeaders.UpdateQuestRanking(from, p);
    }

    public static int GetCredits(Mobile m)
    {
        int val = 0;

        XmlQuestPoints p = (XmlQuestPoints)XmlAttach.FindAttachment(m, typeof(XmlQuestPoints));
        if (p != null)
        {
            val = p.Credits;
        }

        return val;
    }

    public static int GetPoints(Mobile m)
    {
        int val = 0;

        XmlQuestPoints p = (XmlQuestPoints)XmlAttach.FindAttachment(m, typeof(XmlQuestPoints));
        if (p != null)
        {
            val = p.Points;
        }

        return val;
    }

    public static bool HasCredits(Mobile m, int credits)
    {
        if (m == null || m.Deleted)
        {
            return false;
        }

        XmlQuestPoints p = (XmlQuestPoints)XmlAttach.FindAttachment(m, typeof(XmlQuestPoints));

        if (p != null)
        {
            if (p.Credits >= credits)
            {
                return true;
            }
        }

        return false;
    }

    public static bool TakeCredits(Mobile m, int credits)
    {
        if (m == null || m.Deleted)
        {
            return false;
        }

        XmlQuestPoints p = (XmlQuestPoints)XmlAttach.FindAttachment(m, typeof(XmlQuestPoints));

        if (p != null)
        {
            if (p.Credits >= credits)
            {
                p.Credits -= credits;
                return true;
            }
        }

        return false;
    }

    public override void Serialize(IGenericWriter writer)
    {
        base.Serialize(writer);

        writer.Write(0);
        // version 0
        writer.Write(m_Points);
        writer.Write(m_Credits);
        writer.Write(m_Completed);
        writer.Write(m_Rank);
        writer.Write(m_DeltaRank);
        writer.Write(m_WhenRanked);

        // save the quest history
        if (QuestList != null)
        {
            writer.Write(QuestList.Count);

            foreach(QuestEntry e in QuestList)
            {
                e.Serialize(writer);
            }
        }
        else
        {
            writer.Write(0);
        }

        // need this in order to rebuild the rankings on deser
        if (AttachedTo is Mobile)
        {
            writer.Write(AttachedTo as Mobile);
        }
        else
        {
            writer.Write((Mobile)null);
        }
    }

    public override void Deserialize(IGenericReader reader)
    {
        base.Deserialize(reader);

        int version = reader.ReadInt();

        switch (version)
        {
            case 0:
                {
                    m_Points = reader.ReadInt();
                    m_Credits = reader.ReadInt();
                    m_Completed = reader.ReadInt();
                    m_Rank = reader.ReadInt();
                    m_DeltaRank = reader.ReadInt();
                    m_WhenRanked = reader.ReadDateTime();

                    int nquests = reader.ReadInt();

                    if (nquests > 0)
                    {
                        QuestList = new ArrayList(nquests);
                        for(int i = 0; i< nquests;i++)
                        {
                            QuestEntry e = new QuestEntry();
                            e.Deserialize(reader);

                            QuestList.Add(e);
                        }
                    }



                    // get the owner of this in order to rebuild the rankings
                    Mobile quester = reader.ReadEntity<Mobile>();

                    // rebuild the ranking list
                    // if they have never made a kill, then dont rank
                    if (quester != null && QuestsCompleted > 0)
                    {
                        XmlQuestLeaders.UpdateQuestRanking(quester, this);
                    }
                    break;
                }
        }
    }

    public override string OnIdentify(Mobile from) => $"Quest Points Status:\nTotal Quest Points = {Points}\nTotal Quests Completed = {QuestsCompleted}\nQuest Credits Available = {Credits}";
}
