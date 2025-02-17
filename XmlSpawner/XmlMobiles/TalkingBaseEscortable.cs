using System;
using System.Collections;
using System.Collections.Generic;
using Server.Items;
using Server.ContextMenus;
using EDI = Server.Mobiles.EscortDestinationInfo;
using Server.Engines.XmlSpawner2;

namespace Server.Mobiles;

public class TalkingBaseEscortable : TalkingBaseCreature
{
    private EDI m_Destination;
    public string m_DestinationString;

    private DateTime m_DeleteTime;
    private Timer m_DeleteTimer;

    public override bool Commandable => false; // Our master cannot boss us around!

    [CommandProperty(AccessLevel.GameMaster)]
    public string Destination
    {
        get => m_Destination?.Name;
        set
        {
            m_DestinationString = value;
            m_Destination = EDI.Find(value);

            // if the destination cant be found in the current EDI list then try to add it
            if (value == null || value.Length <= 0)
            {
                return;
            }

            if (m_Destination == null)
            {
                if (Region.Regions.Count == 0) // after world load, before region load
                {
                    return;
                }

                foreach (Region region in Region.Regions)
                {
                    if (string.Compare(region.Name, value, true) == 0)
                    {
                        EDI newedi = new EDI(value, region);

                        m_Destination = newedi;
                        return;
                    }
                }
            }

        }
    }

    private static string[] m_TownNames = new string[]
    {
        "Cove", "Britain", "Jhelom",
        "Minoc", "Ocllo", "Trinsic",
        "Vesper", "Yew", "Skara Brae",
        "Nujel'm", "Moonglow", "Magincia"
    };

    public static new void Initialize()
    {
        foreach (Mobile m in World.Mobiles.Values)
        {
            if (m is TalkingBaseEscortable creature)
            {
                // reestablish the DialogAttachment assignment
                XmlDialog xa = (XmlDialog)XmlAttach.FindAttachment(m, typeof(XmlDialog));
                creature.DialogAttachment = xa;

                // initialize Destination after world load (now, regions are loaded)
                creature.Destination = creature.m_DestinationString;
            }
        }
    }
    [Constructible]
    public TalkingBaseEscortable()  : this (-1)
    {
    }

    [Constructible]
    public TalkingBaseEscortable(int gender) : base(AIType.AI_Melee, FightMode.Aggressor, 22, 1, 0.2, 1.0)
    {
        InitBody(gender);
        InitOutfit();
    }

    public virtual void InitBody(int gender)
    {
        SetStr(90, 100);
        SetDex(90, 100);
        SetInt(15, 25);

        Hue = Race.RandomSkinHue();


        switch (gender)
        {
            case -1:
                {
                    Female = Utility.RandomBool(); break;
                }
            case 0:
                {
                    Female = false; break;
                }
            case 1:
                {
                    Female = true; break;
                }
        }

        if (Female)
        {
            Body = 401;
            Name = NameList.RandomName("female");
        }
        else
        {
            Body = 400;
            Name = NameList.RandomName("male");
        }
    }

    public virtual void InitOutfit()
    {
        AddItem(new FancyShirt(Utility.RandomNeutralHue()));
        AddItem(new ShortPants(Utility.RandomNeutralHue()));
        AddItem(new Boots(Utility.RandomNeutralHue()));
        HairHue = Race.RandomHairHue();
        PackGold(200, 250);
    }

    public virtual bool SayDestinationTo(Mobile m)
    {
        EDI dest = GetDestination();

        if (dest == null || !m.Alive)
        {
            return false;
        }

        Mobile escorter = GetEscorter();

        if (escorter == null)
        {
            Say("I am looking to go to {0}, will you take me?", dest.Name == "Ocllo" && m.Map == Map.Trammel ? "Haven" : dest.Name);
            return true;
        }

        if (escorter == m)
        {
            Say("Lead on! Payment will be made when we arrive in {0}.", dest.Name == "Ocllo" && m.Map == Map.Trammel ? "Haven" : dest.Name);
            return true;
        }

        return false;
    }

    private static Hashtable m_EscortTable = new();

    public static Hashtable EscortTable => m_EscortTable;

    private static TimeSpan m_EscortDelay = TimeSpan.FromMinutes(5.0);

    public virtual bool AcceptEscorter(Mobile m)
    {
        EDI dest = GetDestination();

        if (dest == null)
        {
            return false;
        }

        Mobile escorter = GetEscorter();

        if (escorter != null || !m.Alive)
        {
            return false;
        }

        TalkingBaseEscortable escortable = (TalkingBaseEscortable)m_EscortTable[m];

        if (escortable != null && !escortable.Deleted && escortable.GetEscorter() == m)
        {
            Say("I see you already have an escort.");
            return false;
        }

        if (m is PlayerMobile mobile && mobile.LastEscortTime + m_EscortDelay >= DateTime.Now)
        {
            int minutes = (int)Math.Ceiling((mobile.LastEscortTime + m_EscortDelay - DateTime.Now).TotalMinutes);

            Say("You must rest {0} minute{1} before we set out on this journey.", minutes, minutes == 1 ? "" : "s");
            return false;
        }
        if (SetControlMaster(m))
        {
            m_LastSeenEscorter = DateTime.Now;

            if (m is PlayerMobile playerMobile)
            {
                playerMobile.LastEscortTime = DateTime.Now;
            }

            Say("Lead on! Payment will be made when we arrive in {0}.", dest.Name == "Ocllo" && m.Map == Map.Trammel ? "Haven" : dest.Name );
            m_EscortTable[m] = this;
            StartFollow();
            return true;
        }

        return false;
    }

    public override bool HandlesOnSpeech(Mobile from)
    {
        if (from.InRange(Location, 3))
        {
            return true;
        }

        return base.HandlesOnSpeech(from);
    }

    public override void OnSpeech(SpeechEventArgs e)
    {
        base.OnSpeech(e);

        EDI dest = GetDestination();

        if (dest != null && !e.Handled && e.Mobile.InRange(Location, 3))
        {
            if (e.HasKeyword(0x1D)) // *destination*
            {
                e.Handled = SayDestinationTo(e.Mobile);
            }
            else if (e.HasKeyword(0x1E)) // *i will take thee*
            {
                e.Handled = AcceptEscorter(e.Mobile);
            }
        }
    }

    public override void OnAfterDelete()
    {
        if (m_DeleteTimer != null)
        {
            m_DeleteTimer.Stop();
        }

        m_DeleteTimer = null;

        base.OnAfterDelete();
    }

    public override void OnThink()
    {
        base.OnThink();
        CheckAtDestination();
    }

    protected override bool OnMove(Direction d)
    {
        if (!base.OnMove(d))
        {
            return false;
        }

        CheckAtDestination();

        return true;
    }

    public virtual void StartFollow()
    {
        StartFollow(GetEscorter());
    }

    public virtual void StartFollow(Mobile escorter)
    {
        if (escorter == null)
        {
            return;
        }

        ActiveSpeed = 0.1;
        PassiveSpeed = 0.2;

        ControlOrder = OrderType.Follow;
        ControlTarget = escorter;

        CurrentSpeed = 0.1;
    }

    public virtual void StopFollow()
    {
        ActiveSpeed = 0.2;
        PassiveSpeed = 1.0;

        ControlOrder = OrderType.None;
        ControlTarget = null;

        CurrentSpeed = 1.0;
    }

    private DateTime m_LastSeenEscorter;

    public virtual Mobile GetEscorter()
    {
        if (!Controlled)
        {
            return null;
        }

        Mobile master = ControlMaster;

        if (master == null)
        {
            return null;
        }

        if (master.Deleted || master.Map != Map || !master.InRange(Location, 30) || !master.Alive)
        {
            StopFollow();

            TimeSpan lastSeenDelay = DateTime.Now - m_LastSeenEscorter;

            if (lastSeenDelay >= TimeSpan.FromMinutes(2.0))
            {
                master.SendLocalizedMessage(1042473); // You have lost the person you were escorting.
                Say(1005653);                         // Hmmm.  I seem to have lost my master.

                SetControlMaster(null);
                m_EscortTable.Remove(master);

                Timer.DelayCall(TimeSpan.FromSeconds(5.0), Delete);
                return null;
            }

            ControlOrder = OrderType.Stay;
            return master;
        }

        if (ControlOrder != OrderType.Follow)
        {
            StartFollow(master);
        }

        m_LastSeenEscorter = DateTime.Now;
        return master;
    }

    public virtual void BeginDelete()
    {
        if (m_DeleteTimer != null)
        {
            m_DeleteTimer.Stop();
        }

        m_DeleteTime = DateTime.Now + TimeSpan.FromSeconds(30.0);

        m_DeleteTimer = new DeleteTimer(this, m_DeleteTime - DateTime.Now);
        m_DeleteTimer.Start();
    }

    public virtual bool CheckAtDestination()
    {
        EDI dest = GetDestination();

        if (dest == null)
        {
            return false;
        }

        Mobile escorter = GetEscorter();

        if (escorter == null)
        {
            return false;
        }

        if (dest.Contains(Location))
        {
            Say(1042809, escorter.Name); // We have arrived! I thank thee, ~1_PLAYER_NAME~! I have no further need of thy services. Here is thy pay.


            // not going anywhere
            m_Destination = null;
            m_DestinationString = null;

            Container cont = escorter.Backpack;

            if (cont == null)
            {
                cont = escorter.BankBox;
            }

            Gold gold = new Gold(500, 1000);

            if (cont == null || !cont.TryDropItem(escorter, gold, false))
            {
                gold.MoveToWorld(escorter.Location, escorter.Map);
            }

            Misc.Titles.AwardFame(escorter, 10, true);

            bool gainedPath = false;

            PlayerMobile pm = escorter as PlayerMobile;

            if (pm != null)
            {
                if (pm.CompassionGains > 0 && DateTime.Now > pm.NextCompassionDay)
                {
                    pm.NextCompassionDay = DateTime.MinValue;
                    pm.CompassionGains = 0;
                }

                if (pm.CompassionGains >= 5) // have already gained 5 points in one day, can gain no more
                {
                    pm.SendLocalizedMessage(1053004); // You must wait about a day before you can gain in compassion again.
                }
                else if (VirtueHelper.Award(pm, VirtueName.Compassion, 1, ref gainedPath))
                {
                    if (gainedPath)
                    {
                        pm.SendLocalizedMessage(1053005); // You have achieved a path in compassion!
                    }
                    else
                    {
                        pm.SendLocalizedMessage(1053002); // You have gained in compassion.
                    }

                    pm.NextCompassionDay = DateTime.Now + TimeSpan.FromDays(1.0); // in one day CompassionGains gets reset to 0
                    ++pm.CompassionGains;
                }
                else
                {
                    pm.SendLocalizedMessage(1053003); // You have achieved the highest path of compassion and can no longer gain any further.
                }
            }

            XmlQuest.RegisterEscort(this, escorter);

            StopFollow();
            SetControlMaster(null);
            m_EscortTable.Remove(escorter);
            BeginDelete();

            return true;
        }

        return false;
    }

    public TalkingBaseEscortable(Serial serial) : base(serial)
    {
    }

    public override void Serialize(IGenericWriter writer)
    {
        base.Serialize(writer);

        writer.Write(0); // version

        EDI dest = GetDestination();

        writer.Write(dest != null);

        if (dest != null)
        {
            writer.Write(dest.Name);
        }

        writer.Write(m_DeleteTimer != null);

        if (m_DeleteTimer != null)
        {
            writer.WriteDeltaTime(m_DeleteTime);
        }
    }

    public override void Deserialize(IGenericReader reader)
    {
        base.Deserialize(reader);

        int version = reader.ReadInt();

        if (reader.ReadBool())
        {
            m_DestinationString = reader.ReadString(); // NOTE: We cannot EDI.Find here, regions have not yet been loaded :-(
        }

        if (reader.ReadBool())
        {
            m_DeleteTime = reader.ReadDeltaTime();
            m_DeleteTimer = new DeleteTimer(this, m_DeleteTime - DateTime.Now);
            m_DeleteTimer.Start();
        }
    }

    public override bool CanBeRenamedBy(Mobile from) => from.AccessLevel >= AccessLevel.GameMaster;

    public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
    {
        EDI dest = GetDestination();

        if (dest != null && from.Alive)
        {
            Mobile escorter = GetEscorter();

            if (escorter == null || escorter == from)
            {
                list.Add(new AskTalkingDestinationEntry(this, from));
            }

            if (escorter == null)
            {
                list.Add(new AcceptTalkingEscortEntry(this, from));
            }
            else if (escorter == from)
            {
                list.Add(new AbandonTalkingEscortEntry(this, from));
            }
        }

        base.AddCustomContextEntries(from, list);
    }

    public virtual string[] GetPossibleDestinations() => m_TownNames;

    public virtual string PickRandomDestination()
    {
        if (Map.Felucca.Regions.Count == 0 || Map == null || Map == Map.Internal || Location == Point3D.Zero)
        {
            return null; // Not yet fully initialized
        }

        string[] possible = GetPossibleDestinations();
        string picked = null;

        while (picked == null)
        {
            picked = possible[Utility.Random(possible.Length)];
            EDI test = EDI.Find(picked);

            if (test != null && test.Contains(Location))
            {
                picked = null;
            }
        }

        return picked;
    }

    public EDI GetDestination()
    {
        if (m_DestinationString == null && m_DeleteTimer == null)
        {
            m_DestinationString = PickRandomDestination();
        }

        if (m_Destination != null && m_Destination.Name == m_DestinationString)
        {
            return m_Destination;
        }

        if (Map.Felucca.Regions.Count > 0)
        {
            return m_Destination = EDI.Find(m_DestinationString);
        }

        return m_Destination = null;
    }

    private class DeleteTimer : Timer
    {
        private Mobile m_Mobile;

        public DeleteTimer(Mobile m, TimeSpan delay) : base(delay) => m_Mobile = m;

        protected override void OnTick()
        {
            m_Mobile.Delete();
        }
    }
}


public class AskTalkingDestinationEntry : ContextMenuEntry
{
    private TalkingBaseEscortable m_Mobile;
    private Mobile m_From;

    public AskTalkingDestinationEntry(TalkingBaseEscortable m, Mobile from) : base(6100, 3)
    {
        m_Mobile = m;
        m_From = from;
    }

    public override void OnClick()
    {
        m_Mobile.SayDestinationTo(m_From);
    }
}

public class AcceptTalkingEscortEntry : ContextMenuEntry
{
    private TalkingBaseEscortable m_Mobile;
    private Mobile m_From;

    public AcceptTalkingEscortEntry(TalkingBaseEscortable m, Mobile from) : base(6101, 3)
    {
        m_Mobile = m;
        m_From = from;
    }

    public override void OnClick()
    {
        m_Mobile.AcceptEscorter(m_From);
    }
}

public class AbandonTalkingEscortEntry : ContextMenuEntry
{
    private TalkingBaseEscortable m_Mobile;
    private Mobile m_From;

    public AbandonTalkingEscortEntry(TalkingBaseEscortable m, Mobile from) : base(6102, 3)
    {
        m_Mobile = m;
        m_From = from;
    }

    public override void OnClick()
    {
        m_Mobile.Delete(); // OSI just seems to delete instantly
    }
}
