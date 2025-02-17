using System.Collections.Generic;

namespace Server.Mobiles;

public class TalkingJeweler : TalkingBaseVendor
{
    private List<SBInfo> m_SBInfos = new();
    protected override List<SBInfo> SBInfos => m_SBInfos;

    [Constructible]
    public TalkingJeweler() : base("the jeweler")
    {
        SetSkill(SkillName.ItemID, 64.0, 100.0);
    }

    public override void InitSBInfo()
    {
        m_SBInfos.Add(new SBJewel());
    }

    public TalkingJeweler(Serial serial) : base(serial)
    {
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
