using System;
using Server.Items;

namespace Server.Engines.XmlSpawner2;

public interface IXmlAttachment
{
    ASerial Serial { get; }

    string Name { get; set; }

    TimeSpan Expiration { get; set; }

    DateTime ExpirationEnd { get; }

    DateTime CreationTime { get; }

    bool Deleted { get; }

    bool DoDelete { get; set; }

    bool CanActivateInBackpack { get; }

    bool CanActivateEquipped { get; }

    bool CanActivateInWorld { get; }

    bool HandlesOnSpeech { get; }

    void OnSpeech(SpeechEventArgs args);

    bool HandlesOnMovement { get; }

    void OnMovement(MovementEventArgs args);

    bool HandlesOnKill { get; }

    void OnKill(Mobile killed, Mobile killer);

    void OnBeforeKill(Mobile killed, Mobile killer);

    bool HandlesOnKilled { get; }

    void OnKilled(Mobile killed, Mobile killer);

    void OnBeforeKilled(Mobile killed, Mobile killer);

    /*
    bool HandlesOnSkillUse { get; }

    void OnSkillUse(Mobile m, Skill skill, bool success);
    */

    object AttachedTo { get; set; }

    object OwnedBy { get; set; }

    bool CanEquip(Mobile from);

    void OnEquip(Mobile from);

    void OnRemoved(object parent);

    void OnAttach();

    void OnReattach();

    void OnUse(Mobile from);

    void OnUser(object target);

    bool BlockDefaultOnUse(Mobile from, object target);

    bool OnDragLift(Mobile from, Item item);

    string OnIdentify(Mobile from);

    string DisplayedProperties(Mobile from);

    void AddProperties(IPropertyList list);

    string AttachedBy { get; }

    void OnDelete();

    void Delete();

    void InvalidateParentProperties();

    void SetAttachedBy(string name);

    void OnTrigger(object activator, Mobile from);

    void OnWeaponHit(Mobile attacker, Mobile defender, BaseWeapon weapon, int damageGiven);

    int OnArmorHit(Mobile attacker, Mobile defender, Item armor, BaseWeapon weapon, int damageGiven);

    void Serialize(IGenericWriter writer);

    void Deserialize(IGenericReader reader);

}

public abstract class XmlAttachment : IXmlAttachment
{
    // ----------------------------------------------
    // Private fields
    // ----------------------------------------------
    private ASerial m_Serial;

    private string m_Name;

    private object m_AttachedTo;

    private object m_OwnedBy;

    private string m_AttachedBy;

    private bool m_Deleted;

    private AttachmentTimer m_ExpirationTimer;

    private TimeSpan m_Expiration = TimeSpan.Zero; // no expiration by default

    private DateTime m_ExpirationEnd;

    private DateTime m_CreationTime; // when the attachment was made

    // ----------------------------------------------
    // Public properties
    // ----------------------------------------------
    [CommandProperty(AccessLevel.GameMaster)]
    public DateTime CreationTime => m_CreationTime;

    public bool Deleted => m_Deleted;

    public bool DoDelete {
        get => false;
        set
        {
            if (value)
            {
                Delete();
            }
        }

    }

    [CommandProperty(AccessLevel.GameMaster)]
    public int SerialValue => m_Serial.Value;

    public ASerial Serial { get => m_Serial;
        set => m_Serial = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public TimeSpan Expiration
    {
        get
        {
            // if the expiration timer is running then return the remaining time
            if (m_ExpirationTimer != null)
            {
                return m_ExpirationEnd - DateTime.Now;
            }

            return m_Expiration;
        }
        set
        {
            m_Expiration = value;
            // if it is already attached to something then set the expiration timer
            if (m_AttachedTo != null)
            {
                DoTimer(m_Expiration);
            }
        }
    }

    public DateTime ExpirationEnd => m_ExpirationEnd;

    [CommandProperty(AccessLevel.GameMaster)]
    public virtual bool CanActivateInBackpack => true;

    [CommandProperty(AccessLevel.GameMaster)]
    public virtual bool CanActivateEquipped => true;

    [CommandProperty(AccessLevel.GameMaster)]
    public virtual bool CanActivateInWorld => true;

    [CommandProperty(AccessLevel.GameMaster)]
    public virtual bool HandlesOnSpeech => false;

    [CommandProperty(AccessLevel.GameMaster)]
    public virtual bool HandlesOnMovement => false;

    [CommandProperty(AccessLevel.GameMaster)]
    public virtual bool HandlesOnKill => false;

    [CommandProperty(AccessLevel.GameMaster)]
    public virtual bool HandlesOnKilled => false;

    /*
    [CommandProperty(AccessLevel.GameMaster)]
    public virtual bool HandlesOnSkillUse { get{return false; } }
    */

    [CommandProperty(AccessLevel.GameMaster)]
    public virtual string Name { get => m_Name;
        set => m_Name = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public virtual object Attached => m_AttachedTo;

    public virtual object AttachedTo { get => m_AttachedTo;
        set => m_AttachedTo = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public virtual string AttachedBy => m_AttachedBy;

    public virtual object OwnedBy { get => m_OwnedBy;
        set => m_OwnedBy = value;
    }

    [CommandProperty(AccessLevel.GameMaster)]
    public virtual object Owner => m_OwnedBy;

    // ----------------------------------------------
    // Private methods
    // ----------------------------------------------
    private void DoTimer(TimeSpan delay)
    {
        m_ExpirationEnd = DateTime.Now + delay;

        if (m_ExpirationTimer != null)
        {
            m_ExpirationTimer.Stop();
        }

        m_ExpirationTimer = new AttachmentTimer(this, delay);
        m_ExpirationTimer.Start();
    }

    // a timer that can be implement limited lifetime attachments
    private class AttachmentTimer : Timer
    {
        private XmlAttachment m_Attachment;

        public AttachmentTimer(XmlAttachment attachment, TimeSpan delay) : base(delay) => m_Attachment = attachment;

        protected override void OnTick()
        {
            m_Attachment.Delete();
        }
    }

    // ----------------------------------------------
    // Constructors
    // ----------------------------------------------
    public XmlAttachment()
    {
        m_CreationTime = DateTime.Now;

        // get the next unique serial id
        m_Serial = ASerial.NewSerial();

        // register the attachment in the serial keyed hashtable
        XmlAttach.HashSerial(m_Serial, this);
    }

    // needed for deserialization
    public XmlAttachment(ASerial serial) => m_Serial = serial;

    // ----------------------------------------------
    // Public methods
    // ----------------------------------------------

    public static void Initialize()
    {
        XmlAttach.CleanUp();
    }

    public virtual bool CanEquip(Mobile from) => true;

    public virtual void OnEquip(Mobile from)
    {
    }

    public virtual void OnRemoved(object parent)
    {
    }

    public virtual void OnAttach()
    {
        // start up the expiration timer on attachment
        if (m_Expiration > TimeSpan.Zero)
        {
            DoTimer(m_Expiration);
        }
    }

    public virtual void OnReattach()
    {
    }

    public virtual void OnUse(Mobile from)
    {
    }

    public virtual void OnUser(object target)
    {
    }

    public virtual bool BlockDefaultOnUse(Mobile from, object target) => false;

    public virtual bool OnDragLift(Mobile from, Item item) => true;

    public void SetAttachedBy(string name)
    {
        m_AttachedBy = name;
    }

    public virtual void OnSpeech(SpeechEventArgs args)
    {
    }

    public virtual void OnMovement(MovementEventArgs args)
    {
    }

    public virtual void OnKill(Mobile killed, Mobile killer)
    {
    }

    public virtual void OnBeforeKill(Mobile killed, Mobile killer)
    {
    }

    public virtual void OnKilled(Mobile killed, Mobile killer)
    {
    }

    public virtual void OnBeforeKilled(Mobile killed, Mobile killer)
    {
    }

    /*
    public virtual void OnSkillUse(Mobile m, Skill skill, bool success)
    {
    }
    */

    public virtual void OnWeaponHit(Mobile attacker, Mobile defender, BaseWeapon weapon, int damageGiven)
    {
    }

    public virtual int OnArmorHit(Mobile attacker, Mobile defender, Item armor, BaseWeapon weapon, int damageGiven) => 0;

    public virtual string OnIdentify(Mobile from) => null;

    public virtual string DisplayedProperties(Mobile from) => OnIdentify(from);


    public virtual void AddProperties(IPropertyList list)
    {
    }

    public void InvalidateParentProperties()
    {
        if (AttachedTo is Item item)
        {
            ((Item)AttachedTo).InvalidateProperties();
        }
    }

    public void SafeItemDelete(Item item)
    {
        Timer.DelayCall(TimeSpan.Zero, DeleteItemCallback, item);

    }

    public static void DeleteItemCallback(Item item)
    {
        // delete the item
        item?.Delete();
    }

    public static void SafeMobileDelete(Mobile mob)
    {
        Timer.DelayCall(TimeSpan.Zero, DeleteMobileCallback, mob);

    }

    public static void DeleteMobileCallback(Mobile mob)
    {
        // delete the mobile
        mob?.Delete();
    }

    public void Delete()
    {
        if (m_Deleted)
        {
            return;
        }

        m_Deleted = true;

        if (m_ExpirationTimer != null)
        {
            m_ExpirationTimer.Stop();
        }

        OnDelete();

        // dereference the attachment object
        AttachedTo = null;
        OwnedBy = null;
    }

    public virtual void OnDelete()
    {
    }

    public virtual void OnTrigger(object activator, Mobile from)
    {
    }

    public virtual void Serialize(IGenericWriter writer)
    {
        writer.Write(2);
        // version 2
        writer.Write(m_AttachedBy);
        // version 1
        if (OwnedBy is Item item)
        {
            writer.Write(0);
            writer.Write((Item)OwnedBy);
        }
        else
        if (OwnedBy is Mobile mobile)
        {
            writer.Write(1);
            writer.Write((Mobile)OwnedBy);
        }
        else
        {
            writer.Write(-1);
        }

        // version 0
        writer.Write(Name);
        // if there are any active timers, then serialize
        writer.Write(m_Expiration);
        if (m_ExpirationTimer != null)
        {
            writer.Write(m_ExpirationEnd - DateTime.Now);
        }
        else
        {
            writer.Write(TimeSpan.Zero);
        }
        writer.Write(m_CreationTime);
    }

    public virtual void Deserialize(IGenericReader reader)
    {
        int version = reader.ReadInt();

        switch (version)
        {
            case 2:
                {
                    m_AttachedBy = reader.ReadString();
                    goto case 1;
                }
            case 1:
                {
                    int owned = reader.ReadInt();
                    if (owned == 0)
                    {
                        OwnedBy = reader.ReadEntity<Item>();
                    }
                    else
                    if (owned == 1)
                    {
                        OwnedBy = reader.ReadEntity<Mobile>();
                    }
                    else
                    {
                        OwnedBy = null;
                    }

                    goto case 0;
                }
            case 0:
                {
                    // version 0
                    Name = reader.ReadString();
                    m_Expiration = reader.ReadTimeSpan();
                    TimeSpan remaining = reader.ReadTimeSpan();

                    if (remaining > TimeSpan.Zero)
                    {
                        DoTimer(remaining);
                    }

                    m_CreationTime = reader.ReadDateTime();
                    break;
                }
        }
    }
}
