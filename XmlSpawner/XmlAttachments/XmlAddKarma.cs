using System;
using Server.Mobiles;

namespace Server.Engines.XmlSpawner2;

public class XmlAddKarma : XmlAttachment
{
    private int m_DataValue; // default data

    [CommandProperty(AccessLevel.GameMaster)]
    public int Value
    {
        get => m_DataValue;
        set => m_DataValue = value;
    }

    // These are the various ways in which the message attachment can be constructed.
    // These can be called via the [addatt interface, via scripts, via the spawner ATTACH keyword.
    // Other overloads could be defined to handle other types of arguments

    // a serial constructor is REQUIRED
    public XmlAddKarma(ASerial serial) : base(serial)
    {
    }

    [Attachable]
    public XmlAddKarma(int value) => Value = value;


    public override void Serialize(IGenericWriter writer)
    {
        base.Serialize(writer);

        writer.Write(0);
        // version 0
        writer.Write(m_DataValue);

    }

    public override void Deserialize(IGenericReader reader)
    {
        base.Deserialize(reader);

        int version = reader.ReadInt();
        // version 0
        m_DataValue = reader.ReadInt();
    }

    public override void OnAttach()
    {
        base.OnAttach();

        // apply the mod
        if (AttachedTo is PlayerMobile mobile)
        {
            // for players just add it immediately
            mobile.Karma += Value;
            mobile.SendMessage("Receive {0}",OnIdentify((Mobile)AttachedTo));

            // and then remove the attachment
            Timer.DelayCall(TimeSpan.Zero, Delete);
            //Delete();
        }
        else if (AttachedTo is Item)
        {
            // dont allow item attachments
            Delete();
        }

    }

    public override bool HandlesOnKilled => true;

    public override void OnKilled(Mobile killed, Mobile killer)
    {
        base.OnKilled(killed, killer);

        if (killer == null)
        {
            return;
        }

        killer.Karma += Value;

        killer.SendMessage("Receive {0}",OnIdentify(killer));
    }


    public override string OnIdentify(Mobile from) => $"{Value} Karma";
}
