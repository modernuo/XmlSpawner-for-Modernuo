using System;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Engines.XmlSpawner2;

public class XmlUse : XmlAttachment
{
    private bool m_BlockDefaultUse;
    private string m_Condition;         // additional condition required for use
    private string m_TargetingAction;    // action performed when the target cursor is brought up
    private string m_TargetCondition;   // condition test applied when target is selected to determine whether it is appropriate
    private string m_TargetFailureAction;     // action performed if target condition is not met
    private string m_SuccessAction;     // action performed on successful use or targeting
    private string m_FailureAction;     // action performed if the player cannot use the object for reasons other than range, refractory, or maxuses
    private string m_RefractoryAction;  // action performed if the object is used before the refractory interval expires
    private string m_MaxUsesAction;     // action performed if the object is used when the maxuses are exceeded
    private int m_NUses;
    private int m_MaxRange = 3;        // must be within 3 tiles to use by default
    private int m_MaxTargetRange = 30; // must be within 30 tiles to target by default
    private int m_MaxUses;
    private TimeSpan m_Refractory = TimeSpan.Zero;
    public DateTime m_EndTime;
    private bool m_RequireLOS;
    private bool m_AllowCarried = true;
    private bool m_TargetingEnabled;


    [CommandProperty(AccessLevel.GameMaster)]
    public bool TargetingEnabled
    {
        get => m_TargetingEnabled;
        set => m_TargetingEnabled = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool AllowCarried
    {
        get => m_AllowCarried;
        set => m_AllowCarried = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool RequireLOS
    {
        get => m_RequireLOS;
        set => m_RequireLOS = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int MaxRange
    {
        get => m_MaxRange;
        set => m_MaxRange = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int MaxTargetRange
    {
        get => m_MaxTargetRange;
        set => m_MaxTargetRange = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int NUses
    {
        get => m_NUses;
        set => m_NUses = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int MaxUses
    {
        get => m_MaxUses;
        set => m_MaxUses = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public TimeSpan Refractory
    {
        get => m_Refractory;
        set => m_Refractory = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public bool BlockDefaultUse
    {
        get => m_BlockDefaultUse;
        set => m_BlockDefaultUse = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public string Condition
    {
        get => m_Condition;
        set => m_Condition = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public string TargetCondition
    {
        get => m_TargetCondition;
        set => m_TargetCondition = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public string TargetingAction
    {
        get => m_TargetingAction;
        set => m_TargetingAction = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public string TargetFailureAction
    {
        get => m_TargetFailureAction;
        set => m_TargetFailureAction = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public string SuccessAction
    {
        get => m_SuccessAction;
        set => m_SuccessAction = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public string FailureAction
    {
        get => m_FailureAction;
        set => m_FailureAction = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public string RefractoryAction
    {
        get => m_RefractoryAction;
        set => m_RefractoryAction = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public string MaxUsesAction
    {
        get => m_MaxUsesAction;
        set => m_MaxUsesAction = value;
    }

    public XmlUse(ASerial serial) : base(serial)
    {
    }
    [Attachable]
    public XmlUse()
    {
    }

    [Attachable]
    public XmlUse(int maxuses) => MaxUses = maxuses;

    [Attachable]
    public XmlUse(int maxuses, double refractory)
    {
        MaxUses = maxuses;
        Refractory = TimeSpan.FromSeconds(refractory);
    }

    public override void Serialize(IGenericWriter writer)
    {
        base.Serialize(writer);

        writer.Write(3);
        // version 3
        writer.Write(m_MaxTargetRange);
        // version 2
        writer.Write(m_TargetingEnabled);
        writer.Write(m_TargetingAction);
        writer.Write(m_TargetCondition);
        writer.Write(m_TargetFailureAction);
        // version 1
        writer.Write(m_AllowCarried);
        // version 0
        writer.Write(m_RequireLOS);
        writer.Write(m_MaxRange);
        writer.Write(m_Refractory);
        writer.Write(m_EndTime - DateTime.Now);
        writer.Write(m_MaxUses);
        writer.Write(m_NUses);
        writer.Write(m_BlockDefaultUse);
        writer.Write(m_Condition);
        writer.Write(m_SuccessAction);
        writer.Write(m_FailureAction);
        writer.Write(m_RefractoryAction);
        writer.Write(m_MaxUsesAction);
    }

    public override void Deserialize(IGenericReader reader)
    {
        base.Deserialize(reader);

        int version = reader.ReadInt();
        switch (version)
        {
            case 3:
                {
                    m_MaxTargetRange = reader.ReadInt();
                    goto case 2;
                }
            case 2:
                {
                    m_TargetingEnabled = reader.ReadBool();
                    m_TargetingAction = reader.ReadString();
                    m_TargetCondition = reader.ReadString();
                    m_TargetFailureAction = reader.ReadString();
                    goto case 1;
                }
            case 1:
                {
                    m_AllowCarried = reader.ReadBool();
                    goto case 0;
                }
            case 0:
                {
                    // version 0
                    m_RequireLOS = reader.ReadBool();
                    m_MaxRange = reader.ReadInt();
                    Refractory = reader.ReadTimeSpan();
                    TimeSpan remaining = reader.ReadTimeSpan();
                    m_EndTime = DateTime.Now + remaining;
                    m_MaxUses = reader.ReadInt();
                    m_NUses = reader.ReadInt();
                    m_BlockDefaultUse = reader.ReadBool();
                    m_Condition = reader.ReadString();
                    m_SuccessAction = reader.ReadString();
                    m_FailureAction = reader.ReadString();
                    m_RefractoryAction = reader.ReadString();
                    m_MaxUsesAction = reader.ReadString();
                    break;
                }
        }
    }

    public void ExecuteActions(Mobile mob, object target, string actions)
    {
        if (actions == null || actions.Length <= 0)
        {
            return;
        }
        // execute any action associated with it
        // allow for multiple action strings on a single line separated by a semicolon

        string[] args = actions.Split(';');

        for (int j = 0; j < args.Length; j++)
        {
            ExecuteAction(mob, target, args[j]);
        }

    }

    private void ExecuteAction(Mobile mob, object target, string action)
    {
        if (action == null || action.Length <= 0)
        {
            return;
        }

        string status_str = null;
        XmlSpawner.SpawnObject TheSpawn = new XmlSpawner.SpawnObject(null, 0);

        TheSpawn.TypeName = action;
        string substitutedtypeName = BaseXmlSpawner.ApplySubstitution(null, target, action);
        string typeName = BaseXmlSpawner.ParseObjectType(substitutedtypeName);

        Point3D loc = new Point3D(0, 0, 0);
        Map map = null;


        if (target is Item ti1)
        {
            if (ti1.Parent == null)
            {
                loc = ti1.Location;
                map = ti1.Map;
            }
            else if (ti1.RootParent is Item item)
            {
                loc = item.Location;
                map = item.Map;
            }
            else if (ti1.RootParent is Mobile mobile)
            {
                loc = mobile.Location;
                map = mobile.Map;
            }

        }
        else if (target is Mobile ti)
        {
            loc = ti.Location;
            map = ti.Map;

        }

        if (BaseXmlSpawner.IsTypeOrItemKeyword(typeName))
        {
            BaseXmlSpawner.SpawnTypeKeyword(target, TheSpawn, typeName, substitutedtypeName, mob, map, out status_str);
        }
        else
        {
            // its a regular type descriptor so find out what it is
            Type type = AssemblyHandler.FindTypeByName(typeName);
            try
            {
                string[] arglist = BaseXmlSpawner.ParseString(substitutedtypeName, 3, "/");
                object o = XmlSpawner.CreateObject(type, arglist[0]);

                if (o == null)
                {
                    status_str = $"invalid type specification: {arglist[0]}";
                }
                else
                if (o is Mobile mobile)
                {
                    if (mobile is BaseCreature creature)
                    {
                        creature.Home = loc; // Spawners location is the home point
                    }

                    mobile.Location = loc;
                    mobile.Map = map;

                    BaseXmlSpawner.ApplyObjectStringProperties(null, substitutedtypeName, mobile, mob, target, out status_str);
                }
                else
                if (o is Item item)
                {
                    BaseXmlSpawner.AddSpawnItem(null, target, TheSpawn, item, loc, map, mob, false, substitutedtypeName, out status_str);
                }
            }
            catch { }
        }

        ReportError(mob, status_str);
    }

    private void ReportError(Mobile mob, string status_str)
    {
        if (status_str != null && mob != null && !mob.Deleted && mob is PlayerMobile && mob.AccessLevel > AccessLevel.Player)
        {
            mob.SendMessage(33, $"{Name}:{status_str}");
        }
    }

    // return true to allow use
    private bool CheckCondition(Mobile from, object target)
    {
        // test the condition if there is one
        if (Condition != null && Condition.Length > 0)
        {
            string status_str;

            return BaseXmlSpawner.CheckPropertyString(null, target, Condition, out status_str);
        }

        return true;
    }

    // return true to allow use
    private bool CheckTargetCondition(Mobile from, object target)
    {
        // test the condition if there is one
        if (TargetCondition != null && TargetCondition.Length > 0)
        {
            string status_str;

            return BaseXmlSpawner.CheckPropertyString(null, target, TargetCondition, out status_str);
        }

        return true;
    }

    // return true to allow use
    private bool CheckRange(Mobile from, object target)
    {
        if (from == null || !(target is IEntity entity) || MaxRange < 0)
        {
            return false;
        }

        Map map = entity.Map;
        Point3D loc = entity.Location;

        if (map != from.Map)
        {
            return false;
        }

        // check for allowed use in pack
        if (entity is Item targetitem)
        {
            // is it carried by the user?
            if (targetitem.RootParent == from)
            {
                return AllowCarried;
            }

            // block use in other containers or on other mobiles
            if (targetitem.Parent != null)
            {
                return false;
            }
        }

        bool haslos = true;
        if (RequireLOS)
        {
            // check los as well
            haslos = from.InLOS(entity);
        }

        return from.InRange(loc, MaxRange) && haslos;
    }

    public bool CheckMaxUses
    {
        get
        {
            // is there a use limit?
            if (MaxUses > 0 && NUses >= MaxUses)
            {
                return false;
            }

            return true;
        }
    }

    public bool CheckRefractory
    {
        get
        {
            // is there a refractory limit?
            // if it is still refractory then return
            if (Refractory > TimeSpan.Zero && DateTime.Now < m_EndTime)
            {
                return false;
            }

            return true;
        }
    }

    public void OutOfRange(Mobile from)
    {
        if (from == null)
        {
            return;
        }

        from.SendLocalizedMessage(500446); // That is too far away.
    }

    public class XmlUseTarget : Target
    {
        private object m_objectused;
        private XmlUse m_xa;

        public XmlUseTarget(int range, object objectused, XmlUse xa)
            : base(range, true, TargetFlags.None)
        {
            m_objectused = objectused;
            m_xa = xa;
            CheckLOS = false;
        }
        protected override void OnTarget(Mobile from, object targeted)
        {
            if (from == null || targeted == null || m_xa == null)
            {
                return;
            }

            // success
            if (m_xa.CheckTargetCondition(from, targeted))
            {
                m_xa.ExecuteActions(from, targeted, m_xa.SuccessAction);

                m_xa.m_EndTime = DateTime.Now + m_xa.Refractory;
                m_xa.NUses++;
            }
            else
            {
                m_xa.ExecuteActions(from, targeted, m_xa.TargetFailureAction);
            }

        }
    }

    private void TryToTarget(Mobile from, object target, XmlUse xa)
    {
        if (from == null)
        {
            return;
        }

        ExecuteActions(from, target, TargetingAction);

        if (xa != null)
        {
            from.Target = new XmlUseTarget(xa.MaxTargetRange, target, xa);
        }
    }

    private void TryToUse(Mobile from, object target)
    {
        if (CheckRange(from, target) && CheckCondition(from, target) && CheckMaxUses && CheckRefractory)
        {
            // check for targeting
            if (TargetingEnabled)
            {
                TryToTarget(from, target, this);
            }
            else
            {
                // success
                ExecuteActions(from, target, SuccessAction);

                m_EndTime = DateTime.Now + Refractory;
                NUses++;
            }
        }
        else
        {
            // failure
            if (!CheckRange(from, target))
            {
                OutOfRange(from);
            }
            else if (!CheckRefractory)
            {
                ExecuteActions(from, target, RefractoryAction);
            }
            else if (!CheckMaxUses)
            {
                ExecuteActions(from, target, MaxUsesAction);
            }

            else
            {
                ExecuteActions(from, target, FailureAction);
            }
        }
    }

    // disable the default use of the target
    public override bool BlockDefaultOnUse(Mobile from, object target) => BlockDefaultUse || !(CheckRange(from, target) && CheckCondition(from, target) && CheckMaxUses && CheckRefractory);

    // this is called when the attachment is on the user
    public override void OnUser(object target)
    {
        Mobile from = AttachedTo as Mobile;

        TryToUse(from, target);

    }


    // this is called when the attachment is on the target being used
    public override void OnUse(Mobile from)
    {
        object target = AttachedTo;

        // if a target tries to use itself, then ignore it, it will be handled by OnUser
        if (target == from)
        {
            return;
        }

        TryToUse(from, target);
    }
}
