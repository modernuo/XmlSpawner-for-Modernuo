using System;

namespace Server.Engines.XmlSpawner2;

public class XmlHue : XmlAttachment
{
    private int m_Originalhue;
    private int m_Hue;

    [CommandProperty(AccessLevel.GameMaster)]
    public int Hue
    {
        get => m_Hue;
        set => m_Hue = value;
    }


    // These are the various ways in which the message attachment can be constructed.
    // These can be called via the [addatt interface, via scripts, via the spawner ATTACH keyword.
    // Other overloads could be defined to handle other types of arguments

    // a serial constructor is REQUIRED
    public XmlHue(ASerial serial) : base(serial)
    {
    }

    [Attachable]
    public XmlHue(int value)
    {
        m_Hue = value;
        Expiration = TimeSpan.FromSeconds(30.0); // default 30 second duration
    }

    [Attachable]
    public XmlHue(int value, double duration)
    {
        m_Hue = value;
        Expiration = TimeSpan.FromMinutes(duration);
    }

    public override void Serialize(IGenericWriter writer)
    {
        base.Serialize(writer);

        writer.Write(0);
        // version 0
        writer.Write(m_Originalhue);
        writer.Write(m_Hue);
    }

    public override void Deserialize(IGenericReader reader)
    {
        base.Deserialize(reader);

        int version = reader.ReadInt();
        // version 0

        m_Originalhue = reader.ReadInt();
        m_Hue = reader.ReadInt();
    }

    public override string OnIdentify(Mobile from)
    {
        base.OnIdentify(from);

        if (from == null || from.AccessLevel == AccessLevel.Player)
        {
            return null;
        }

        if (Expiration > TimeSpan.Zero)
        {
            return $"Hue {m_Hue} expires in {Expiration.TotalMinutes} mins";
        }

        return $"Hue {m_Hue}";
    }

    public override void OnDelete()
    {
        base.OnDelete();

        // remove the mod
        if (AttachedTo is Mobile mobile)
        {
            mobile.Hue = m_Originalhue;
        }
        else if (AttachedTo is Item item)
        {
            item.Hue = m_Originalhue;
        }
    }

    public override void OnAttach()
    {
        base.OnAttach();

        // apply the mod
        if (AttachedTo is Mobile)
        {
            Mobile m = AttachedTo as Mobile;
            m_Originalhue = m.Hue;
            m.Hue = m_Hue;
        }
        else
        if (AttachedTo is Item)
        {
            Item i = AttachedTo as Item;
            m_Originalhue = i.Hue;
            i.Hue = m_Hue;
        }
        else
        {
            Delete();
        }
    }

}
