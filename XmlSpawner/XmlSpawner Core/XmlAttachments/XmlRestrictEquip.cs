using System;
using Server.Mobiles;

namespace Server.Engines.XmlSpawner2;

public class XmlRestrictEquip : XmlAttachment
{
    private string m_TestValue;          // default Test condition
    private string m_FailMsg;            // message given when equipping fails
    private string m_PropertyListString; // string displayed in the properties list

    // a serial constructor is REQUIRED
    public XmlRestrictEquip(ASerial serial)
        : base(serial)
    {
    }

    [Attachable]
    public XmlRestrictEquip() => Test = String.Empty;

    [Attachable]
    public XmlRestrictEquip(string name)
    {
        Name = name;
        Test = String.Empty;
    }

    [Attachable]
    public XmlRestrictEquip(string name, string test)
    {
        Name = name;
        Test = test;
    }

    [Attachable]
    public XmlRestrictEquip(string name, string test, double expiresin)
    {
        Name = name;
        Test = test;
        Expiration = TimeSpan.FromMinutes(expiresin);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public string Test
    {
        get => m_TestValue;
        set => m_TestValue = value;
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string FailMsg
    {
        get => m_FailMsg;
        set => m_FailMsg = value;
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string PropertyListString
    {
        get => m_PropertyListString;
        set
        {
            m_PropertyListString = value;
            InvalidateParentProperties();
        }
    }
    // These are the various ways in which the message attachment can be constructed.
    // These can be called via the [addatt interface, via scripts, via the spawner ATTACH keyword.
    // Other overloads could be defined to handle other types of arguments
    public override bool CanEquip(Mobile from)
    {
        if (from == null)
        {
            return false;
        }

        bool allowequip = true;

        // test the condition if there is one
        if (Test != null && Test.Length > 0)
        {
            string status_str;

            allowequip = BaseXmlSpawner.CheckPropertyString(null, AttachedTo, Test, out status_str);

            if (!allowequip && FailMsg != null)
            {
                from.SendMessage(FailMsg);
            }
        }

        return allowequip;
    }

    public override void Serialize(IGenericWriter writer)
    {
        base.Serialize(writer);

        writer.Write(1);
        // version 1
        writer.Write(m_PropertyListString);
        writer.Write(m_FailMsg);
        // version 0
        writer.Write(m_TestValue);
    }

    public override void Deserialize(IGenericReader reader)
    {
        base.Deserialize(reader);

        int version = reader.ReadInt();
        switch (version)
        {
            case 1:
                {
                    m_PropertyListString = reader.ReadString();
                    m_FailMsg = reader.ReadString();
                    goto case 0;
                }
            case 0:
                {
                    m_TestValue = reader.ReadString();
                    break;
                }
        }
    }

    public override string DisplayedProperties(Mobile from) => PropertyListString;

    public override string OnIdentify(Mobile from)
    {
        if (from == null || from.AccessLevel < AccessLevel.Counselor)
        {
            return null;
        }

        if (Expiration > TimeSpan.Zero)
        {
            return $"{Name}: RestrictEquip '{Test}' expires in {Expiration.TotalMinutes} mins";
        }

        return $"{Name}: RestrictEquip '{Test}'";
    }
}
