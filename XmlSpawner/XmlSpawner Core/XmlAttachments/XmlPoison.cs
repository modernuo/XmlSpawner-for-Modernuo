namespace Server.Engines.XmlSpawner2;

public class XmlPoison : XmlAttachment
{
    private int p_level;
    // a serial constructor is REQUIRED
    public XmlPoison(ASerial serial)
        : base(serial)
    {
    }

    [Attachable]
    public XmlPoison(int level) => p_level = level;

    // when attached to a mobile, it should gain poison immunity and a poison

    //attack, but no poisoning skill
    public Poison PoisonImmune
    {
        get
        {
            if (p_level < 1)
            {
                return Poison.Lesser;
            }

            if (p_level == 1)
            {
                return Poison.Regular;
            }
            if (p_level == 2)
            {
                return Poison.Greater;
            }
            if (p_level == 3)
            {
                return Poison.Deadly;
            }
            if (p_level > 3)
            {
                return Poison.Lethal;
            }
            return Poison.Regular;
        }
    }
    public Poison HitPoison
    {
        get
        {
            if (p_level < 1)
            {
                return Poison.Lesser;
            }

            if (p_level == 1)
            {
                return Poison.Regular;
            }
            if (p_level == 2)
            {
                return Poison.Greater;
            }
            if (p_level == 3)
            {
                return Poison.Deadly;
            }
            if (p_level > 3)
            {
                return Poison.Lethal;
            }
            return Poison.Regular;
        }
    }
    public override void Serialize(IGenericWriter writer)
    {
        base.Serialize(writer);

        writer.Write(0);
        // version 0
        writer.Write(p_level);
    }

    public override void Deserialize(IGenericReader reader)
    {
        base.Deserialize(reader);

        int version = reader.ReadInt();
        switch(version)
        {
            case 0:
                {
                    // version 0
                    p_level = reader.ReadInt();
                    break;
                }
        }
    }
}
