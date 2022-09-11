using System;
using Server.Mobiles;

namespace Server.Engines.XmlSpawner2;

public class XmlIsEnemy : XmlAttachment
{
    private string m_TestString; // Test condition to see if mobile is an enemy of the object this is attached to
    public XmlIsEnemy(ASerial serial) : base(serial)
    {
    }

    [Attachable]
    public XmlIsEnemy() => Test = String.Empty;

    [Attachable]
    public XmlIsEnemy(string name)
    {
        Name = name;
        Test = String.Empty;
    }

    [Attachable]
    public XmlIsEnemy(string name, string test)
    {
        Name = name;
        Test = test;
    }

    [Attachable]
    public XmlIsEnemy(string name, string test, double expiresin)
    {
        Name = name;
        Test = test;
        Expiration = TimeSpan.FromMinutes(expiresin);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public string Test
    {
        get => m_TestString;
        set => m_TestString = value;
    }
    public bool IsEnemy(Mobile from)
    {
        if (from == null)
        {
            return false;
        }

        bool isenemy = false;

        // test the condition if there is one
        if (Test != null && Test.Length > 0)
        {
            string status_str;

            isenemy = BaseXmlSpawner.CheckPropertyString(null, AttachedTo, Test, out status_str);
        }

        return isenemy;
    }

    public override void Serialize(IGenericWriter writer)
    {
        base.Serialize(writer);

        writer.Write(0);
        // version 0
        writer.Write(m_TestString);
    }

    public override void Deserialize(IGenericReader reader)
    {
        base.Deserialize(reader);

        int version = reader.ReadInt();
        switch (version)
        {
            case 0:
                {
                    m_TestString = reader.ReadString();
                    break;
                }
        }
    }

    public override string OnIdentify(Mobile from)
    {
        if (from == null || from.AccessLevel < AccessLevel.Counselor)
        {
            return null;
        }

        if (Expiration > TimeSpan.Zero)
        {
            return $"{Name}: IsEnemy '{Test}' expires in {Expiration.TotalMinutes} mins";
        }

        return $"{Name}: IsEnemy '{Test}'";
    }
}
