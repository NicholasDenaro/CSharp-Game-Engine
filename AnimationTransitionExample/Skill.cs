﻿using GameEngine;
using GameEngine._2D;
using GameEngine.Interfaces;
using System.Collections.Generic;
using System.Drawing;

namespace AnimationTransitionExample
{
    public class Skill : HotbarAction
    {
        public delegate bool SkillAction(Location location, IDescription description);

        public int Level { get; private set; }

        public string Name { get; private set; }

        public SkillIcon Icon { get; private set; }

        public SkillAction SAction { get; private set; }

        public bool CanMove { get; private set; }

        public Skill(string name, SkillIcon icon, SkillAction action, bool canMove)
        {
            this.Name = name;
            this.SAction = action;
            this.Icon = icon;
            this.CanMove = canMove;
            SkillManager.Instance.Add(this);
        }

        protected Skill()
        {
        }

        public void Action(LivingEntity entity)
        {
            entity.SetPreppedSkill(this);
        }

        protected virtual Skill CreateNew(Skill skill)
        {
            skill.Name = Name;
            skill.Icon = Icon;
            skill.SAction = SAction;
            skill.CanMove = CanMove;

            return skill;
        }

        public virtual Skill CreateNew()
        {
            Skill skill = new Skill();
            return CreateNew(skill);
        }

        public Image Image()
        {
            return Icon.Image();
        }
    }

    public class SkillIcon : Description2D
    {
        public SkillIcon(int xIndex, int yIndex)
        {
            Sprite sprite = Sprite.Sprites["skills"];
            this.DrawAction = () => sprite.GetImage(yIndex * sprite.HImages + xIndex);
        }
    }

    public class SkillManager
    {
        private static SkillManager singleton;
        public static SkillManager Instance
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new SkillManager();
                }

                return singleton;
            }
        }

        private Dictionary<string, Skill> skills;
        private SkillManager()
        {
            skills = new Dictionary<string, Skill>();
        }

        public void Add(Skill skill)
        {
            skills.Add(skill.Name, skill);
        }

        public Skill this[string name]
        {
            get
            {
                return skills[name].CreateNew();
            }
        }
    }
}