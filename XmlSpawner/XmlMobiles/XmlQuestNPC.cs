using Server.Items;
using CPA = Server.CommandPropertyAttribute;


namespace Server.Mobiles;

public class XmlQuestNPC : TalkingBaseCreature
{

    [Constructible]
    public XmlQuestNPC() : this(-1)
    {
    }

    [Constructible]
    public XmlQuestNPC(int gender) : base(AIType.AI_Melee, FightMode.None, 10, 1, 0.8, 3.0)
    {
        SetStr(10, 30);
        SetDex(10, 30);
        SetInt(10, 30);

        Fame = 50;
        Karma = 50;

        CanHearGhosts = true;

        SpeechHue = Utility.RandomDyedHue();
        Title = string.Empty;
        Hue = Race.RandomSkinHue();

        Female = gender switch
        {
            -1 => Utility.RandomBool(),
            0  => false,
            1  => true,
            _  => Female
        };

        if (Female)
        {
            Body = 0x191;
            Name = NameList.RandomName("female");
            Item hair = new Item(Utility.RandomList(0x203B, 0x203C, 0x203D, 0x2045, 0x204A, 0x2046 , 0x2049));
            hair.Hue =Race.RandomHairHue();
            hair.Layer = Layer.Hair;
            hair.Movable = false;
            AddItem(hair);
            Item hat = Utility.Random(5) switch //4 hats, one empty, for no hat
            {
                0 => new FloppyHat(Utility.RandomNeutralHue()),
                1 => new FeatheredHat(Utility.RandomNeutralHue()),
                2 => new Bonnet(),
                3 => new Cap(Utility.RandomNeutralHue()),
                _ => null
            };

            AddItem(hat);
            Item pants = Utility.Random(3) switch
            {
                0 => new ShortPants(GetRandomHue()),
                1 => new LongPants(GetRandomHue()),
                2 => new Skirt(GetRandomHue()),
                _ => null
            };
            AddItem(pants);
            Item shirt = Utility.Random(7) switch
            {
                0 => new Doublet(GetRandomHue()),
                1 => new Surcoat(GetRandomHue()),
                2 => new Tunic(GetRandomHue()),
                3 => new FancyDress(GetRandomHue()),
                4 => new PlainDress(GetRandomHue()),
                5 => new FancyShirt(GetRandomHue()),
                6 => new Shirt(GetRandomHue()),
                _ => null
            };
            AddItem(shirt);
        }
        else
        {
            Body = 0x190;
            Name = NameList.RandomName("male");
            Item hair = new Item(Utility.RandomList(0x203B, 0x203C, 0x203D, 0x2044, 0x2045, 0x2047, 0x2048));
            hair.Hue = Race.RandomHairHue();
            hair.Layer = Layer.Hair;
            hair.Movable = false;
            AddItem(hair);
            Item beard = new Item(Utility.RandomList(0x0000, 0x203E, 0x203F, 0x2040, 0x2041, 0x2067, 0x2068, 0x2069));
            beard.Hue = hair.Hue;
            beard.Layer = Layer.FacialHair;
            beard.Movable = false;
            AddItem(beard);
            Item hat = Utility.Random(7) switch //6 hats, one empty, for no hat
            {
                0 => new SkullCap(GetRandomHue()),
                1 => new Bandana(GetRandomHue()),
                2 => new WideBrimHat(),
                3 => new TallStrawHat(Utility.RandomNeutralHue()),
                4 => new StrawHat(Utility.RandomNeutralHue()),
                5 => new TricorneHat(Utility.RandomNeutralHue()),
                _ => null
            };
            AddItem(hat);
            Item pants = Utility.Random(2) switch
            {
                0 => new ShortPants(GetRandomHue()),
                1 => new LongPants(GetRandomHue()),
                _ => null
            };
            AddItem(pants);
            Item shirt = Utility.Random(5) switch
            {
                0 => new Doublet(GetRandomHue()),
                1 => new Surcoat(GetRandomHue()),
                2 => new Tunic(GetRandomHue()),
                3 => new FancyShirt(GetRandomHue()),
                4 => new Shirt(GetRandomHue()),
                _ => null
            };
            AddItem(shirt);
        }

        Item feet = Utility.Random(3) switch
        {
            0 => new Boots(Utility.RandomNeutralHue()),
            1 => new Shoes(Utility.RandomNeutralHue()),
            2 => new Sandals(Utility.RandomNeutralHue()),
            _ => null
        };
        AddItem(feet);
        Container pack = new Backpack();

        pack.DropItem(new Gold(0, 50));

        pack.Movable = false;

        AddItem(pack);
    }

    public XmlQuestNPC(Serial serial) : base(serial)
    {
    }



    private static int GetRandomHue()
    {
        switch (Utility.Random(6))
        {
            default:
            case 0:
                {
                    return 0;
                }
            case 1:
                {
                    return Utility.RandomBlueHue();
                }
            case 2:
                {
                    return Utility.RandomGreenHue();
                }
            case 3:
                {
                    return Utility.RandomRedHue();
                }
            case 4:
                {
                    return Utility.RandomYellowHue();
                }
            case 5:
                {
                    return Utility.RandomNeutralHue();
                }
        }
    }


    public override void Serialize(IGenericWriter writer)
    {
        base.Serialize(writer);

        writer.Write(0); // version

    }

    public override void Deserialize(IGenericReader reader)
    {
        base.Deserialize(reader);

        int version = reader.ReadInt();

    }
}
