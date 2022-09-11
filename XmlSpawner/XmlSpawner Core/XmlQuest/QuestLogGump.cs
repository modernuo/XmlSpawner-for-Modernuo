using System;
using System.Collections;
using Server.Items;
using Server.Engines.XmlSpawner2;

//
// XmlLogGump
// modified from RC0 BOBGump.cs
//
namespace Server.Gumps;

public class QuestLogGump : Gump
{
    private Mobile m_From;

    private ArrayList m_List;

    private int m_Page;

    private const int LabelColor = 0x7FFF;

    public int GetIndexForPage(int page)
    {
        int index = 0;

        while (page-- > 0)
        {
            index += GetCountForIndex(index);
        }

        return index;
    }

    public int GetCountForIndex(int index)
    {
        int slots = 0;
        int count = 0;

        ArrayList list = m_List;

        for (int i = index; i >= 0 && i < list.Count; ++i)
        {
            object obj = list[i];

            int add;

            add = 1;

            if (slots + add > 10)
            {
                break;
            }

            slots += add;

            ++count;
        }

        return count;
    }


    public QuestLogGump(Mobile from) : this(from, 0, null)
    {
    }

    public override void OnResponse(Network.NetState sender, RelayInfo info)
    {
        if (info == null || m_From == null)
        {
            return;
        }

        switch (info.ButtonID)
        {
            case 0: // EXIT
                {
                    break;
                }

            case 2: // Previous page
                {
                    if (m_Page > 0)
                    {
                        m_From.SendGump(new QuestLogGump(m_From, m_Page - 1, m_List));
                    }

                    return;
                }
            case 3: // Next page
                {
                    if (GetIndexForPage(m_Page + 1) < m_List.Count)
                    {
                        m_From.SendGump(new QuestLogGump(m_From, m_Page + 1, m_List));
                    }

                    break;
                }
            case 10: // Top players
                {
                    // if this player has an XmlQuestPoints attachment, find it
                    XmlQuestPoints p = (XmlQuestPoints)XmlAttach.FindAttachment(m_From,typeof(XmlQuestPoints));

                    m_From.CloseGump<XmlQuestLeaders.TopQuestPlayersGump>();
                    m_From.SendGump(new XmlQuestLeaders.TopQuestPlayersGump(p));

                    break;
                }


            default:
                {
                    if (info.ButtonID >= 2000)
                    {
                        int index = info.ButtonID - 2000;

                        if (index < 0 || index >= m_List.Count)
                        {
                            break;
                        }

                        if (m_List[index] is IXmlQuest)
                        {
                            IXmlQuest o = m_List[index] as IXmlQuest;

                            if (o != null && !o.Deleted)
                            {
                                m_From.SendGump(new QuestLogGump(m_From, m_Page, null));
                                m_From.CloseGump(typeof(XmlQuestStatusGump));
                                m_From.SendGump(new XmlQuestStatusGump(o, o.TitleString, 320, 0, true));
                            }
                        }
                    }

                    break;
                }
        }
    }


    public QuestLogGump(Mobile from, int page, ArrayList list) : base(12, 24)
    {
        if (from == null)
        {
            return;
        }

        from.CloseGump(typeof(QuestLogGump));

        XmlQuestPoints p = (XmlQuestPoints)XmlAttach.FindAttachment(from, typeof(XmlQuestPoints));

        m_From = from;
        m_Page = page;

        if (list == null)
        {
            // make a new list based on the number of items in the book
            int nquests = 0;
            list = new ArrayList();

            // find all quest items in the players pack
            if (from.Backpack != null)
            {
                Item [] packquestitems = from.Backpack.FindItemsByType(typeof(IXmlQuest));

                if (packquestitems != null)
                {
                    nquests += packquestitems.Length;
                    for (int i = 0; i < packquestitems.Length; ++i)
                    {
                        if (packquestitems[i] != null && !packquestitems[i].Deleted && !(packquestitems[i].Parent is XmlQuestBook))
                        {
                            list.Add(packquestitems[i]);
                        }
                    }
                }

                // find any questbooks they might have
                Item [] questbookitems = from.Backpack.FindItemsByType(typeof(XmlQuestBook));

                if (questbookitems != null)
                {

                    for (int j = 0; j < questbookitems.Length; ++j)
                    {
                        Item [] questitems = ((XmlQuestBook)questbookitems[j]).FindItemsByType(typeof(IXmlQuest));

                        if (questitems != null)
                        {
                            nquests += questitems.Length;

                            for (int i = 0; i < questitems.Length; ++i)
                            {
                                list.Add(questitems[i]);
                            }
                        }
                    }
                }

                // find any completed quests on the XmlQuestPoints attachment

                if (p != null && p.QuestList != null)
                {
                    // add all completed quests
                    foreach(XmlQuestPoints.QuestEntry q in p.QuestList)
                    {
                        list.Add(q);
                    }
                }
            }

        }

        m_List = list;

        int index = GetIndexForPage(page);
        int count = GetCountForIndex(index);

        int tableIndex = 0;

        int width = 600;

        width = 766;

        X = (824 - width) / 2;

        int xoffset = 20;

        AddPage(0);

        AddBackground(10, 10, width, 439, 5054);
        AddImageTiled(18, 20, width - 17, 420, 2624);

        AddImageTiled(58 - xoffset, 64, 36, 352, 200);   // open
        AddImageTiled(96 - xoffset, 64, 163, 352, 1416); // name
        AddImageTiled(261 - xoffset, 64, 55, 352, 200);  // type
        AddImageTiled(308 - xoffset, 64, 85, 352, 1416); // status
        AddImageTiled(395 - xoffset, 64, 116, 352, 200); // expires

        AddImageTiled(511 - xoffset, 64, 42, 352, 1416); // points
        AddImageTiled(555 - xoffset, 64, 175, 352, 200); // completed
        AddImageTiled(734 - xoffset, 64, 42, 352, 1416); // repeated

        for (int i = index; i < index + count && i >= 0 && i < list.Count; ++i)
        {
            object obj = list[i];

            AddImageTiled(24, 94 + tableIndex * 32, 489, 2, 2624);

            ++tableIndex;
        }

        AddAlphaRegion(18, 20, width - 17, 420);
        AddImage(5, 5, 10460);
        AddImage(width - 15, 5, 10460);
        AddImage(5, 424, 10460);
        AddImage(width - 15, 424, 10460);

        AddHtmlLocalized(375, 25, 200, 30, 1046026, LabelColor); // Quest Log

        AddHtmlLocalized(63 - xoffset, 45, 200, 32, 1072837, LabelColor); // Current Points:

        AddHtml(243 - xoffset, 45, 200, 32, XmlSimpleGump.Color("Available Credits:","FFFFFF")); // Your Reward Points:

        AddHtml(453 - xoffset, 45, 200, 32, XmlSimpleGump.Color("Rank:","FFFFFF")); // Rank

        AddHtml(600 - xoffset, 45, 200, 32, XmlSimpleGump.Color("Quests Completed:","FFFFFF")); // Quests completed

        if (p != null)
        {

            int pcolor = 53;
            AddLabel(170 - xoffset, 45, pcolor, p.Points.ToString());
            AddLabel(350 - xoffset, 45, pcolor, p.Credits.ToString());
            AddLabel(500 - xoffset, 45, pcolor, p.Rank.ToString());
            AddLabel(720 - xoffset, 45, pcolor, p.QuestsCompleted.ToString());
        }

        AddHtmlLocalized(63 - xoffset, 64, 200, 32, 3000362, LabelColor);  // Open
        AddHtmlLocalized(147 - xoffset, 64, 200, 32, 3005104, LabelColor); // Name
        AddHtmlLocalized(270 - xoffset, 64, 200, 32, 1062213, LabelColor); // Type
        AddHtmlLocalized(326 - xoffset, 64, 200, 32, 3000132, LabelColor); // Status
        AddHtmlLocalized(429 - xoffset, 64, 200, 32, 1062465, LabelColor); // Expires

        AddHtml(514 - xoffset, 64, 200, 32, XmlSimpleGump.Color("Points","FFFFFF")); // Points
        AddHtml(610 - xoffset, 64, 200, 32, XmlSimpleGump.Color("Next Available","FFFFFF")); // Next Available
        //AddHtmlLocalized(610 - xoffset, 64, 200, 32,  1046033, LabelColor, false, false); // Completed
        AddHtmlLocalized(738 - xoffset, 64, 200, 32, 3005020, LabelColor); // Repeat

        AddButton(675 - xoffset, 416, 4017, 4018, 0);
        AddHtmlLocalized(710 - xoffset, 416, 120, 20, 1011441, LabelColor); // EXIT

        AddButton(113 - xoffset, 416, 0xFA8, 0xFAA, 10);
        AddHtml(150 - xoffset, 416, 200, 32, XmlSimpleGump.Color("Top Players","FFFFFF")); // Top players gump


        tableIndex = 0;

        if (page > 0)
        {
            AddButton(225, 416, 4014, 4016, 2);
            AddHtmlLocalized(260, 416, 150, 20, 1011067, LabelColor); // Previous page
        }

        if (GetIndexForPage(page + 1) < list.Count)
        {
            AddButton(375, 416, 4005, 4007, 3);
            AddHtmlLocalized(410, 416, 150, 20, 1011066, LabelColor); // Next page
        }

        for (int i = index; i < index + count && i >= 0 && i < list.Count; ++i)
        {
            object obj = list[i];


            if (obj is IXmlQuest quest)
            {
                int y = 96 + tableIndex++ * 32;


                AddButton(60 - xoffset, y + 2, 0xFAB, 0xFAD, 2000 + i); // open gump


                int color;

                if (!quest.IsValid)
                {
                    color = 33;
                }
                else
                if (quest.IsCompleted)
                {
                    color = 67;
                }
                else
                {
                    color = 5;
                }


                AddLabel(100 - xoffset, y, color, quest.Name);

                //AddHtmlLocalized(315, y, 200, 32, e.IsCompleted ? 1049071 : 1049072, htmlcolor, false, false); // Completed/Incomplete
                AddLabel(315 - xoffset, y, color, quest.IsCompleted ? "Completed" : "In Progress");

                // indicate the expiration time
                if (quest.IsValid)
                {

                    // do a little parsing of the expiration string to fit it in the space
                    string substring = quest.ExpirationString;
                    if (quest.ExpirationString.IndexOf("Expires in") >= 0)
                    {
                        substring = quest.ExpirationString.Substring(11);
                    }
                    AddLabel(400 - xoffset, y, color, substring);
                }
                else
                {
                    AddLabel(400 - xoffset, y, color, "No longer valid");
                }

                if (quest.PartyEnabled)
                {

                    AddLabel(270 - xoffset, y, color, "Party");
                    //AddHtmlLocalized(250, y, 200, 32, 3000332, htmlcolor, false, false); // Party
                }
                else
                {

                    AddLabel(270 - xoffset, y, color, "Solo");
                }

                AddLabel(515 - xoffset, y, color, quest.Difficulty.ToString());

            }
            else
            if (obj is XmlQuestPoints.QuestEntry entry)
            {
                int y = 96 + tableIndex++ * 32;
                int color = 67;

                AddLabel(100 - xoffset, y, color, entry.Name);

                AddLabel(315 - xoffset, y, color, "Completed");

                if (entry.PartyEnabled)
                {

                    AddLabel(270 - xoffset, y, color, "Party");
                    //AddHtmlLocalized(250, y, 200, 32, 3000332, htmlcolor, false, false); // Party
                }
                else
                {

                    AddLabel(270 - xoffset, y, color, "Solo");
                }

                AddLabel(515 - xoffset, y, color, entry.Difficulty.ToString());

                //AddLabel(560 - xoffset, y, color, e.WhenCompleted.ToString());
                // determine when the quest can be done again by looking for an xmlquestattachment with the same name
                XmlQuestAttachment qa = (XmlQuestAttachment)XmlAttach.FindAttachment(from, typeof(XmlQuestAttachment), entry.Name);
                if (qa != null)
                {
                    if (qa.Expiration == TimeSpan.Zero)
                    {
                        AddLabel(560 - xoffset, y, color, "Not Repeatable");
                    }
                    else
                    {
                        DateTime nexttime = DateTime.Now + qa.Expiration;
                        AddLabel(560 - xoffset, y, color, nexttime.ToString());
                    }
                }
                else
                {
                    // didnt find one so it can be done again
                    AddLabel(560 - xoffset, y, color, "Available Now");
                }

                AddLabel(741 - xoffset, y, color, entry.TimesCompleted.ToString());
            }
        }
    }
}
