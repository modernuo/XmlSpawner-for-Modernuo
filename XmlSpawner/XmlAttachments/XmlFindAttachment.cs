﻿using Server.Mobiles;

namespace Server.Engines.XmlSpawner2;

public class XmlFindAttachment : XmlAttachment
{
    private XmlFindGump[] m_FindGumps = new XmlFindGump[3]{null,null,null};

    // a serial constructor is REQUIRED
    public XmlFindAttachment(ASerial serial) : base(serial)
    {
    }

    public XmlFindAttachment()
    {
    }

    public void AddResults(XmlFindGump gump)
    {
        m_FindGumps[2]=m_FindGumps[1];
        m_FindGumps[1]=m_FindGumps[0];
        m_FindGumps[0]=gump;
    }

    public XmlFindGump GetResult(int val)
    {
        val-=1;
        if (val<m_FindGumps.Length)
        {
            return m_FindGumps[val];
        }

        return null;
    }

    public override void OnAttach()
    {
        if (!(AttachedTo is PlayerMobile))
        {
            Delete();
        }
    }

    //Non si può salvare il risultato, che sarebbe inattendibile dopo un riavvio, comunque
    public override void Serialize(IGenericWriter writer)
    {
    }
    public override void Deserialize(IGenericReader reader)
    {
    }
}
