using Server.Mobiles;

/*
** XmlQuestMaker
**
** Version 1.00
** updated 9/03/04
** ArteGordon
**
*/
namespace Server.Items;

public class XmlQuestMaker : Item
{

    public XmlQuestMaker(Serial serial) : base(serial)
    {
    }


    [Constructible]
    public XmlQuestMaker() : base(0xED4)
    {
        Name = "XmlQuestMaker";
        Movable = false;
        Visible = true;
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

    public override void OnDoubleClick(Mobile from)
    {
        base.OnDoubleClick(from);

        if (!(from is PlayerMobile mobile))
        {
            return;
        }

        // make a quest note
        QuestHolder newquest = new QuestHolder();
        newquest.PlayerMade = true;
        newquest.Creator = mobile;
        newquest.Hue = 500;
        mobile.AddToBackpack(newquest);
        mobile.SendMessage("A blank quest has been added to your pack!");

    }

}
