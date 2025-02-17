using System;
using System.Collections;
using Server.Mobiles;

/*
** SimpleLever, SimpleSwitch, and CombinationLock
** Version 1.03
** updated 5/06/04
** ArteGordon
**
*/
namespace Server.Items;

public interface ILinkable
{
    Item Link { set; get; }
    void Activate(Mobile from, int state, ArrayList links);
}

public class SimpleLever : Item, ILinkable
{
    public enum leverType { Two_State, Three_State }

    private int m_LeverState;
    private leverType m_LeverType = leverType.Two_State;
    private int m_LeverSound = 936;
    private Item m_TargetItem0;
    private string m_TargetProperty0;
    private Item m_TargetItem1;
    private string m_TargetProperty1;
    private Item m_TargetItem2;
    private string m_TargetProperty2;


    private Item m_LinkedItem;
    private bool already_being_activated;

    private bool m_Disabled;

    [CommandProperty(AccessLevel.GameMaster)]
    public bool Disabled
    {
        set => m_Disabled = value;
        get => m_Disabled;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public Item Link
    {
        set => m_LinkedItem = value;
        get => m_LinkedItem;
    }

    [Constructible]
    public SimpleLever()
        : base(0x108C)
    {
        Name = "A lever";
        Movable = false;
    }

    public SimpleLever(Serial serial)
        : base(serial)
    {
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int LeverState
    {
        get => m_LeverState;
        set
        {
            // prevent infinite recursion
            if (!already_being_activated)
            {
                already_being_activated = true;
                Activate(null, value, null);
                already_being_activated = false;
            }

            InvalidateProperties();
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int LeverSound
    {
        get => m_LeverSound;
        set
        {
            m_LeverSound = value;
            InvalidateProperties();
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public leverType LeverType
    {
        get => m_LeverType;
        set
        {
            m_LeverType = value; LeverState = 0;
            InvalidateProperties();
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    new public virtual Direction Direction
    {
        get => base.Direction;
        set {
            base.Direction = value;
            SetLeverStatic();
            InvalidateProperties();
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public Item Target0Item
    {
        get => m_TargetItem0;
        set { m_TargetItem0 = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string Target0Property
    {
        get => m_TargetProperty0;
        set { m_TargetProperty0 = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string Target0ItemName
    {
        get
        {
            if (m_TargetItem0 != null && !m_TargetItem0.Deleted)
            {
                return m_TargetItem0.Name;
            }

            return null;
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public Item Target1Item
    {
        get => m_TargetItem1;
        set { m_TargetItem1 = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string Target1Property
    {
        get => m_TargetProperty1;
        set { m_TargetProperty1 = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string Target1ItemName
    {
        get
        {
            if (m_TargetItem1 != null && !m_TargetItem1.Deleted)
            {
                return
                    m_TargetItem1.Name;
            }

            return null;
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public Item Target2Item
    {
        get => m_TargetItem2;
        set { m_TargetItem2 = value; InvalidateProperties(); }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public string Target2Property
    {
        get => m_TargetProperty2;
        set { m_TargetProperty2 = value; InvalidateProperties(); }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public string Target2ItemName
    {
        get
        {
            if (m_TargetItem2 != null && !m_TargetItem2.Deleted)
            {
                return m_TargetItem2.Name;
            }

            return null;
        }
    }

    public override void Serialize(IGenericWriter writer)
    {
        base.Serialize(writer);

        writer.Write(2); // version
        // version 2
        writer.Write(m_Disabled);
        // version 1
        writer.Write(m_LinkedItem);
        // version 0
        writer.Write(m_LeverState);
        writer.Write(m_LeverSound);
        int ltype = (int)m_LeverType;
        writer.Write(ltype);
        writer.Write(m_TargetItem0);
        writer.Write(m_TargetProperty0);
        writer.Write(m_TargetItem1);
        writer.Write(m_TargetProperty1);
        writer.Write(m_TargetItem2);
        writer.Write(m_TargetProperty2);
    }

    public override void Deserialize(IGenericReader reader)
    {
        base.Deserialize(reader);

        int version = reader.ReadInt();
        switch (version)
        {
            case 2:
                {
                    m_Disabled = reader.ReadBool();
                    goto case 1;
                }
            case 1:
                {
                    m_LinkedItem = reader.ReadEntity<Item>();
                    goto case 0;
                }
            case 0:
                {
                    m_LeverState = reader.ReadInt();
                    m_LeverSound = reader.ReadInt();
                    int ltype = reader.ReadInt();
                    switch (ltype)
                    {
                        case (int)leverType.Two_State:
                            {
                                m_LeverType = leverType.Two_State; break;
                            }
                        case (int)leverType.Three_State:
                            {
                                m_LeverType = leverType.Three_State; break;
                            }
                    }
                    m_TargetItem0 = reader.ReadEntity<Item>();
                    m_TargetProperty0 = reader.ReadString();
                    m_TargetItem1 = reader.ReadEntity<Item>();
                    m_TargetProperty1 = reader.ReadString();
                    m_TargetItem2 = reader.ReadEntity<Item>();
                    m_TargetProperty2 = reader.ReadString();
                }
                break;
        }
    }

    public void SetLeverStatic()
    {

        switch (Direction)
        {
            case Direction.North:
            case Direction.South:
            case Direction.Right:
            case Direction.Up:
                {
                    if (m_LeverType == leverType.Two_State)
                    {
                        ItemID = 0x108c + m_LeverState * 2;
                    }
                    else
                    {
                        ItemID = 0x108c + m_LeverState;
                    }

                    break;
                }
            case Direction.East:
            case Direction.West:
            case Direction.Left:
            case Direction.Down:
                {
                    if (m_LeverType == leverType.Two_State)
                    {
                        ItemID = 0x1093 + m_LeverState * 2;
                    }
                    else
                    {
                        ItemID = 0x1093 + m_LeverState;
                    }

                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    public void Activate(Mobile from, int state, ArrayList links)
    {
        if (Disabled)
        {
            return;
        }

        string status_str = null;

        // assign the lever state
        m_LeverState = state;

        if (m_LeverState < 0)
        {
            m_LeverState = 0;
        }

        if (m_LeverState > 1 && m_LeverType == leverType.Two_State)
        {
            m_LeverState = 1;
        }

        if (m_LeverState > 2)
        {
            m_LeverState = 2;
        }

        // update the graphic
        SetLeverStatic();

        // play the switching sound if possible
        //if (from != null)
        //{
        //	from.PlaySound(m_LeverSound);
        //}
        try
        {
            Effects.PlaySound(Location, Map, m_LeverSound);
        }
        catch { }

        // if a target object has been specified then apply the property modification
        if (m_LeverState == 0 && m_TargetItem0 != null && !m_TargetItem0.Deleted && m_TargetProperty0 != null && m_TargetProperty0.Length > 0)
        {
            BaseXmlSpawner.ApplyObjectStringProperties(null, m_TargetProperty0, m_TargetItem0, from, this, out status_str);
        }
        if (m_LeverState == 1 && m_TargetItem1 != null && !m_TargetItem1.Deleted && m_TargetProperty1 != null && m_TargetProperty1.Length > 0)
        {
            BaseXmlSpawner.ApplyObjectStringProperties(null, m_TargetProperty1, m_TargetItem1, from, this, out status_str);
        }
        if (m_LeverState == 2 && m_TargetItem2 != null && !m_TargetItem2.Deleted && m_TargetProperty2 != null && m_TargetProperty2.Length > 0)
        {
            BaseXmlSpawner.ApplyObjectStringProperties(null, m_TargetProperty2, m_TargetItem2, from, this, out status_str);
        }

        // if the switch is linked, then activate the link as well
        if (Link != null && Link is ILinkable)
        {
            if (links == null)
            {
                links = new ArrayList();
            }
            // activate other linked objects if they have not already been activated
            if (!links.Contains(this))
            {
                links.Add(this);

                ((ILinkable)Link).Activate(from, state, links);
            }
        }

        // report any problems to staff
        if (status_str != null && from != null && from.AccessLevel > AccessLevel.Player)
        {
            from.SendMessage("{0}", status_str);
        }
    }

    public override void OnDoubleClick(Mobile from)
    {
        if (from == null || Disabled)
        {
            return;
        }

        if (!from.InRange(GetWorldLocation(), 2) || !from.InLOS(this))
        {
            from.SendLocalizedMessage(500446); // That is too far away.
            return;
        }

        // change the switch state
        m_LeverState = m_LeverState + 1;

        if (m_LeverState > 1 && m_LeverType == leverType.Two_State)
        {
            m_LeverState = 0;
        }

        if (m_LeverState > 2)
        {
            m_LeverState = 0;
        }

        // carry out the switch actions
        Activate(from, m_LeverState, null);

    }
}

public class SimpleSwitch : Item, ILinkable
{
    private int m_SwitchState;
    private int m_SwitchSound = 939;
    private Item m_TargetItem0;
    private string m_TargetProperty0;
    private Item m_TargetItem1;
    private string m_TargetProperty1;

    private Item m_LinkedItem;
    private bool already_being_activated;

    private bool m_Disabled;

    [CommandProperty(AccessLevel.GameMaster)]
    public bool Disabled
    {
        set => m_Disabled = value;
        get => m_Disabled;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public Item Link
    {
        set => m_LinkedItem = value;
        get => m_LinkedItem;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int SwitchState
    {
        set
        {
            // prevent infinite recursion
            if (!already_being_activated)
            {
                already_being_activated = true;
                Activate(null, value, null);
                already_being_activated = false;
            }

            InvalidateProperties();
        }
        get => m_SwitchState;
    }

    [Constructible]
    public SimpleSwitch()
        : base(0x108F)
    {
        Name = "A switch";
        Movable = false;
    }

    public SimpleSwitch(Serial serial)
        : base(serial)
    {
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int SwitchSound
    {
        get => m_SwitchSound;
        set
        {
            m_SwitchSound = value;
            InvalidateProperties();
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    new public virtual Direction Direction
    {
        get => base.Direction;
        set
        {
            base.Direction = value;
            SetSwitchStatic();
            InvalidateProperties();
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public Item Target0Item
    {
        get => m_TargetItem0;
        set
        {
            m_TargetItem0 = value;
            InvalidateProperties();
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public string Target0Property
    {
        get => m_TargetProperty0;
        set
        {
            m_TargetProperty0 = value;
            InvalidateProperties();
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public string Target0ItemName
    {
        get
        {
            if (m_TargetItem0 != null && !m_TargetItem0.Deleted)
            {
                return m_TargetItem0.Name;
            }

            return null;
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public Item Target1Item
    {
        get => m_TargetItem1;
        set { m_TargetItem1 = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string Target1Property
    {
        get => m_TargetProperty1;
        set
        {
            m_TargetProperty1 = value;
            InvalidateProperties();
        }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string Target1ItemName
    {
        get
        {
            if (m_TargetItem1 != null && !m_TargetItem1.Deleted)
            {
                return m_TargetItem1.Name;
            }

            return null;
        }
    }


    public override void Serialize(IGenericWriter writer)
    {
        base.Serialize(writer);

        writer.Write(2); // version
        // version 2
        writer.Write(m_Disabled);
        // version 1
        writer.Write(m_LinkedItem);
        // version 0
        writer.Write(m_SwitchState);
        writer.Write(m_SwitchSound);
        writer.Write(m_TargetItem0);
        writer.Write(m_TargetProperty0);
        writer.Write(m_TargetItem1);
        writer.Write(m_TargetProperty1);
    }

    public override void Deserialize(IGenericReader reader)
    {
        base.Deserialize(reader);

        int version = reader.ReadInt();
        switch (version)
        {
            case 2:
                {
                    m_Disabled = reader.ReadBool();
                    goto case 1;
                }
            case 1:
                {
                    m_LinkedItem = reader.ReadEntity<Item>();
                    goto case 0;
                }
            case 0:
                {
                    m_SwitchState = reader.ReadInt();
                    m_SwitchSound = reader.ReadInt();
                    m_TargetItem0 = reader.ReadEntity<Item>();
                    m_TargetProperty0 = reader.ReadString();
                    m_TargetItem1 = reader.ReadEntity<Item>();
                    m_TargetProperty1 = reader.ReadString();
                }
                break;
        }
    }

    public void SetSwitchStatic()
    {

        switch (Direction)
        {
            case Direction.North:
            case Direction.South:
            case Direction.Right:
            case Direction.Up:
                {
                    ItemID = 0x108f + m_SwitchState;
                    break;
                }
            case Direction.East:
            case Direction.West:
            case Direction.Left:
            case Direction.Down:
                {
                    ItemID = 0x1091 + m_SwitchState;
                    break;
                }
            default:
                {
                    ItemID = 0x108f + m_SwitchState;
                    break;
                }
        }
    }

    public void Activate(Mobile from, int state, ArrayList links)
    {
        if (Disabled)
        {
            return;
        }

        string status_str = null;

        // assign the switch state
        m_SwitchState = state;

        if (m_SwitchState < 0)
        {
            m_SwitchState = 0;
        }

        if (m_SwitchState > 1)
        {
            m_SwitchState = 1;
        }

        // update the graphic
        SetSwitchStatic();

        //if (from != null)
        //{
        //	from.PlaySound(m_SwitchSound);
        //}
        try
        {
            Effects.PlaySound(Location, Map, m_SwitchSound);
        }
        catch { }

        // if a target object has been specified then apply the property modification
        if (m_SwitchState == 0 && m_TargetItem0 != null && !m_TargetItem0.Deleted && m_TargetProperty0 != null && m_TargetProperty0.Length > 0)
        {
            BaseXmlSpawner.ApplyObjectStringProperties(null, m_TargetProperty0, m_TargetItem0, from, this, out status_str);
        }

        if (m_SwitchState == 1 && m_TargetItem1 != null && !m_TargetItem1.Deleted && m_TargetProperty1 != null && m_TargetProperty1.Length > 0)
        {
            BaseXmlSpawner.ApplyObjectStringProperties(null, m_TargetProperty1, m_TargetItem1, from, this, out status_str);
        }

        // if the switch is linked, then activate the link as well
        if (Link != null && Link is ILinkable)
        {
            if (links == null)
            {
                links = new ArrayList();
            }
            // activate other linked objects if they have not already been activated
            if (!links.Contains(this))
            {
                links.Add(this);

                ((ILinkable)Link).Activate(from, state, links);
            }
        }

        // report any problems to staff
        if (status_str != null && from != null && from.AccessLevel > AccessLevel.Player)
        {
            from.SendMessage("{0}", status_str);
        }
    }

    public override void OnDoubleClick(Mobile from)
    {
        if (from == null || Disabled)
        {
            return;
        }

        if (!from.InRange(GetWorldLocation(), 2) || !from.InLOS(this))
        {
            from.SendLocalizedMessage(500446); // That is too far away.
            return;
        }

        // change the switch state
        m_SwitchState = m_SwitchState + 1;

        if (m_SwitchState > 1)
        {
            m_SwitchState = 0;
        }

        // activate the switch
        Activate(from, m_SwitchState, null);
    }
}

public class CombinationLock : Item
{
    private int m_Combination;
    private Item m_Digit0Object;
    private string m_Digit0Property;
    private Item m_Digit1Object;
    private string m_Digit1Property;
    private Item m_Digit2Object;
    private string m_Digit2Property;
    private Item m_Digit3Object;
    private string m_Digit3Property;
    private Item m_Digit4Object;
    private string m_Digit4Property;
    private Item m_Digit5Object;
    private string m_Digit5Property;
    private Item m_Digit6Object;
    private string m_Digit6Property;
    private Item m_Digit7Object;
    private string m_Digit7Property;
    private Item m_TargetItem;
    private string m_TargetProperty;
    private int m_CombinationSound = 940;

    [Constructible]
    public CombinationLock()
        : base(0x1BBF)
    {
        Name = "A combination lock";
        Movable = false;
    }

    public CombinationLock(Serial serial)
        : base(serial)
    {
    }

    public int SetDigit(int value)
    {
        if (value < 0)
        {
            return 0;
        }

        if (value > 9)
        {
            return 9;
        }

        return value;
    }

    public int CheckDigit(object o, string property)
    {
        if (o == null)
        {
            return 0;
        }

        if (property == null || property.Length <= 0)
        {
            return 0;
        }

        Type ptype;
        int ival = -1;
        string testvalue;
        // check to see whether this is a direct value request, or a test
        string[] argtest = BaseXmlSpawner.ParseString(property, 2, "<>!=");
        if (argtest.Length > 1)
        {
            // ok, its a test, so test it
            string status_str;
            if (BaseXmlSpawner.CheckPropertyString(null, o, property, out status_str))
            {
                return 1; // true
            }

            return 0; // false
        }
        // otherwise get the value of the property requested
        string result = BaseXmlSpawner.GetPropertyValue(null, o, property, out ptype);

        string[] arglist = BaseXmlSpawner.ParseString(result, 2, "=");
        if (arglist.Length < 2)
        {
            return -1;
        }

        string[] arglist2 = BaseXmlSpawner.ParseString(arglist[1], 2, " ");
        if (arglist2.Length > 0)
        {
            testvalue = arglist2[0].Trim();
        }
        else
        {
            return -1;
        }

        if (BaseXmlSpawner.IsNumeric(ptype))
        {
            try
            {
                ival = Convert.ToInt32(testvalue, 10);
            }
            catch { }
        }
        return ival;
    }



    [CommandProperty(AccessLevel.GameMaster)]
    public int Combination
    {
        get => m_Combination;
        set
        {
            m_Combination = value;
            if (m_Combination < 0)
            {
                m_Combination = 0;
            }

            if (m_Combination > 99999999)
            {
                m_Combination = 99999999;
            }

            InvalidateProperties();
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public Item Digit0Object
    {
        get => m_Digit0Object;
        set { m_Digit0Object = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string Digit0Property
    {
        get => m_Digit0Property;
        set { m_Digit0Property = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public int Digit0 => CheckDigit(m_Digit0Object, m_Digit0Property);

    [CommandProperty(AccessLevel.GameMaster)]
    public Item Digit1Object
    {
        get => m_Digit1Object;
        set { m_Digit1Object = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string Digit1Property
    {
        get => m_Digit1Property;
        set { m_Digit1Property = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public int Digit1 => CheckDigit(m_Digit1Object, m_Digit1Property);

    [CommandProperty(AccessLevel.GameMaster)]
    public Item Digit2Object
    {
        get => m_Digit2Object;
        set { m_Digit2Object = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string Digit2Property
    {
        get => m_Digit2Property;
        set { m_Digit2Property = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public int Digit2 => CheckDigit(m_Digit2Object, m_Digit2Property);

    [CommandProperty(AccessLevel.GameMaster)]
    public Item Digit3Object
    {
        get => m_Digit3Object;
        set { m_Digit3Object = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string Digit3Property
    {
        get => m_Digit3Property;
        set { m_Digit3Property = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public int Digit3 => CheckDigit(m_Digit3Object, m_Digit3Property);

    [CommandProperty(AccessLevel.GameMaster)]
    public Item Digit4Object
    {
        get => m_Digit4Object;
        set { m_Digit4Object = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string Digit4Property
    {
        get => m_Digit4Property;
        set { m_Digit4Property = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public int Digit4 => CheckDigit(m_Digit4Object, m_Digit4Property);

    [CommandProperty(AccessLevel.GameMaster)]
    public Item Digit5Object
    {
        get => m_Digit5Object;
        set { m_Digit5Object = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string Digit5Property
    {
        get => m_Digit5Property;
        set { m_Digit5Property = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public int Digit5 => CheckDigit(m_Digit5Object, m_Digit5Property);

    [CommandProperty(AccessLevel.GameMaster)]
    public Item Digit6Object
    {
        get => m_Digit6Object;
        set { m_Digit6Object = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string Digit6Property
    {
        get => m_Digit6Property;
        set { m_Digit6Property = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public int Digit6 => CheckDigit(m_Digit6Object, m_Digit6Property);

    [CommandProperty(AccessLevel.GameMaster)]
    public Item Digit7Object
    {
        get => m_Digit7Object;
        set { m_Digit7Object = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string Digit7Property
    {
        get => m_Digit7Property;
        set { m_Digit7Property = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public int Digit7 => CheckDigit(m_Digit7Object, m_Digit7Property);

    [CommandProperty(AccessLevel.GameMaster)]
    public Item TargetItem
    {
        get => m_TargetItem;
        set { m_TargetItem = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string TargetProperty
    {
        get => m_TargetProperty;
        set { m_TargetProperty = value; InvalidateProperties(); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public string TargetItemName
    { get
    {
        if (m_TargetItem != null && !m_TargetItem.Deleted)
        {
            return m_TargetItem.Name;
        }

        return null;
    } }

    [CommandProperty(AccessLevel.GameMaster)]
    public int CombinationSound
    {
        get => m_CombinationSound;
        set
        {
            m_CombinationSound = value;
            InvalidateProperties();
        }
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool Matched => m_Combination == CurrentValue;

    [CommandProperty(AccessLevel.GameMaster)]

    public int CurrentValue
    {
        get
        {
            int value = Digit0 + Digit1 * 10 + Digit2 * 100 + Digit3 * 1000 + Digit4 * 10000 + Digit5 * 100000 + Digit6 * 1000000 + Digit7 * 10000000;
            return value;
        }
    }


    public override void Serialize(IGenericWriter writer)
    {
        base.Serialize(writer);

        writer.Write(0); // version

        writer.Write(m_Combination);
        writer.Write(m_CombinationSound);
        writer.Write(m_Digit0Object);
        writer.Write(m_Digit0Property);
        writer.Write(m_Digit1Object);
        writer.Write(m_Digit1Property);
        writer.Write(m_Digit2Object);
        writer.Write(m_Digit2Property);
        writer.Write(m_Digit3Object);
        writer.Write(m_Digit3Property);
        writer.Write(m_Digit4Object);
        writer.Write(m_Digit4Property);
        writer.Write(m_Digit5Object);
        writer.Write(m_Digit5Property);
        writer.Write(m_Digit6Object);
        writer.Write(m_Digit6Property);
        writer.Write(m_Digit7Object);
        writer.Write(m_Digit7Property);
        writer.Write(m_TargetItem);
        writer.Write(m_TargetProperty);

    }

    public override void Deserialize(IGenericReader reader)
    {
        base.Deserialize(reader);

        int version = reader.ReadInt();
        switch (version)
        {
            case 0:
                {
                    m_Combination = reader.ReadInt();
                    m_CombinationSound = reader.ReadInt();
                    m_Digit0Object = reader.ReadEntity<Item>();
                    m_Digit0Property = reader.ReadString();
                    m_Digit1Object = reader.ReadEntity<Item>();
                    m_Digit1Property = reader.ReadString();
                    m_Digit2Object = reader.ReadEntity<Item>();
                    m_Digit2Property = reader.ReadString();
                    m_Digit3Object = reader.ReadEntity<Item>();
                    m_Digit3Property = reader.ReadString();
                    m_Digit4Object = reader.ReadEntity<Item>();
                    m_Digit4Property = reader.ReadString();
                    m_Digit5Object = reader.ReadEntity<Item>();
                    m_Digit5Property = reader.ReadString();
                    m_Digit6Object = reader.ReadEntity<Item>();
                    m_Digit6Property = reader.ReadString();
                    m_Digit7Object = reader.ReadEntity<Item>();
                    m_Digit7Property = reader.ReadString();
                    m_TargetItem = reader.ReadEntity<Item>();
                    m_TargetProperty = reader.ReadString();

                }
                break;
        }
    }

    public override void OnDoubleClick(Mobile from)
    {
        if (from == null)
        {
            return;
        }

        if (!from.InRange(GetWorldLocation(), 2) || !from.InLOS(this))
        {
            from.SendLocalizedMessage(500446); // That is too far away.
            return;
        }
        string status_str;
        // test the combination and apply the property to the target item
        if (Matched)
        {
            //from.PlaySound(m_CombinationSound);
            try
            {
                Effects.PlaySound(Location, Map, m_CombinationSound);
            }
            catch { }

            BaseXmlSpawner.ApplyObjectStringProperties(null, m_TargetProperty, m_TargetItem, from, this, out status_str);

        }

    }
}
