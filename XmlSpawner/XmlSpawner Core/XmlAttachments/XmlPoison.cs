namespace Server.Engines.XmlSpawner2;

public class XmlPoison : XmlAttachment
{
    private int p_level;
    // a serial constructor is REQUIRED
    public XmlPoison(ASerial serial) : base(serial)
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
            return p_level switch
            {
                < 1 => Poison.Lesser,
                1   => Poison.Regular,
                2   => Poison.Greater,
                3   => Poison.Deadly,
                > 3 => Poison.Lethal
            };
        }
    }
    public Poison HitPoison
    {
        get
        {
            return p_level switch
            {
                < 1 => Poison.Lesser,
                1   => Poison.Regular,
                2   => Poison.Greater,
                3   => Poison.Deadly,
                > 3 => Poison.Lethal
            };
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
        switch (version)
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
