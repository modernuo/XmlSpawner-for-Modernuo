using System;
using Server.Mobiles;

namespace Server.Engines.XmlSpawner2;

public class XmlIsEnemy : XmlAttachment
{
    private string m_TestString = null; // Test condition to see if mobile is an enemy of the object this is attached to
    public XmlIsEnemy(ASerial serial)
        : base(serial)
    {
    }

    [Attachable]
    public XmlIsEnemy()
    {
        this.Test = String.Empty;
    }

    [Attachable]
    public XmlIsEnemy(string name)
    {
        this.Name = name;
        this.Test = String.Empty;
    }

    [Attachable]
    public XmlIsEnemy(string name, string test)
    {
        this.Name = name;
        this.Test = test;
    }

    [Attachable]
    public XmlIsEnemy(string name, string test, double expiresin)
    {
        this.Name = name;
        this.Test = test;
        this.Expiration = TimeSpan.FromMinutes(expiresin);
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public string Test
    {
        get
        {
            return this.m_TestString;
        }
        set
        {
            this.m_TestString = value;
        }
    }
    public bool IsEnemy(Mobile from)
    {
        if (from == null)
            return false;

        bool isenemy = false;

        // test the condition if there is one
        if (this.Test != null && this.Test.Length > 0)
        {
            string status_str;

            isenemy = BaseXmlSpawner.CheckPropertyString(null, this.AttachedTo, this.Test, out status_str);
        }

        return isenemy;
    }

    public override void Serialize(IGenericWriter writer)
    {
        base.Serialize(writer);

        writer.Write(0);
        // version 0
        writer.Write(this.m_TestString);
    }

    public override void Deserialize(IGenericReader reader)
    {
        base.Deserialize(reader);

        int version = reader.ReadInt();
        switch (version)
        {
            case 0:
                {
                    this.m_TestString = reader.ReadString();
                    break;
                }
        }
    }

    public override string OnIdentify(Mobile from)
    {
        if (from == null || from.AccessLevel < AccessLevel.Counselor)
            return null;

        if (this.Expiration > TimeSpan.Zero)
        {
            return $"{this.Name}: IsEnemy '{this.Test}' expires in {this.Expiration.TotalMinutes} mins";
        }
        else
        {
            return $"{this.Name}: IsEnemy '{this.Test}'";
        }
    }
}
