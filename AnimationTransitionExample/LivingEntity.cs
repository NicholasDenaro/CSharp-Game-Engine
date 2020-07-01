﻿using AnimationTransitionExample.Animations;
using GameEngine;
using GameEngine._2D;
using GameEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace AnimationTransitionExample
{
    public class LivingEntity : Description2D
    {
        protected Stack<AnimationChain> animations;
        public int health;
        public int balance;

        protected int stun;
        private Point knockbackFrom;
        protected AttackCombo combo;

        protected LivingEntity target;
        public LivingEntity Target => target;

        public CombatSkill combatSkill;

        protected int direction;
        private double walkIndex = 0;
        protected int walkCycle;

        public LivingEntity(Sprite sprite, int x, int y, int w, int h) : base(sprite, x, y, w, h)
        {
            health = 100;
            balance = 100;
            knockbackFrom = Point.Empty;
            animations = new Stack<AnimationChain>();
            this.onMove += LivingEntity.WalkIndexing;
        }

        public bool Tick(Location location, Entity entity)
        {
            ImageIndex = direction + ((int)walkIndex % walkCycle);

            if (!IsDead() && balance < 100 && (!animations.Any() || !animations.Peek().Peek().Name.Contains("back")))
            {
                balance++;
            }

            if (combo.IsStarted() && ((animations.Any() && !(animations.Peek().Peek() is AttackAnimation)) || !animations.Any()) && combo.Tick())
            {
                combo.Reset();
            }

            if (stun > 0)
            {
                stun--;
            }

            if (animations.Any())
            {
                if (stun > 0 && animations.Peek().Peek().IsInterruptable())
                {
                    return true;
                }

                if (animations.Peek().Tick(this))
                {
                    animations.Peek().Pop();
                    if (!animations.Peek().Any())
                    {
                        animations.Pop();
                    }
                }

                if (animations.Any() && animations.Peek().Peek().IsPausing())
                {
                    return true;
                }
            }

            if (health <= 0)
            {
                return true;
            }

            return false;
        }

        public static void WalkIndexing(Description2D d2d)
        {
            LivingEntity le = d2d as LivingEntity;
            if(le == null)
            {
                return;
            }

            le.walkIndex += 0.25;
        }

        public static void EndMove(IDescription d)
        {
            LivingEntity le = d as LivingEntity;
            if (le == null)
            {
                return;
            }

            le.walkIndex = 0;
        }

        public void Hit(Description2D hitter, bool finisher, int balanceDiff, int damage)
        {
            if (this is Enemy)
            {
                this.target = hitter as LivingEntity;
            }

            stun = 15;
            DrawOffsetX = 0;
            DrawOffsetY = 0;
            if (animations.Any() && animations.Peek().Peek().IsInterruptable())
            {
                animations.Pop();
                combo.Reset();
            }

            health -= damage;
            balance -= balanceDiff;
            if (balance <= 0 || IsDead())
            {
                if (animations.Any())
                {
                    animations.Pop();
                }

                if (!IsDead())
                {
                    animations.Push(new AnimationChain(AnimationManager.Instance["getup"].MakeInterruptable().MakePausing()));
                }

                animations.Push(new AnimationChain(AnimationManager.Instance[finisher || IsDead() ? "knockback" : "slideback"].MakeInterruptable().MakePausing()));
                knockbackFrom = hitter.Position;
                stun = 0;
            }
        }

        public static void Move(IDescription d)
        {
            LivingEntity le = d as LivingEntity;
            if (le == null)
            {
                return;
            }

            double dir = le.Direction(le.target);
            WalkDirection(le, dir);
            le.ChangeCoordsDelta(Math.Cos(dir), Math.Sin(dir));
        }

        public static void WalkDirection(LivingEntity le, double direction)
        {
            double imgDir = -direction;
            if (imgDir < 0)
            {
                imgDir += Math.PI * 2;
            }

            le.direction = (int)((imgDir + Math.PI / 4) / (Math.PI / 2) + 3) * 4;
        }

        public static void StartAttack(IDescription d)
        {
            LivingEntity le = d as LivingEntity;
            if (le == null)
            {
                return;
            }
        }

        public static void Combo(IDescription d)
        {
            LivingEntity le = d as LivingEntity;
            if (le == null)
            {
                return;
            }

            if (le.combo.CanChain())
            {
                le.combo.Advance();
            }
            else
            {
                le.combo.Reset();
            }
        }

        public static void ResetToAttackPosition(IDescription d)
        {
            LivingEntity le = d as LivingEntity;
            if (le == null)
            {
                return;
            }
            le.DrawOffsetX = 0;
            le.DrawOffsetY = 0;
        }

        public static void Strike(IDescription d, bool finisher, int balance, int damage)
        {
            LivingEntity le = d as LivingEntity;
            if (le != null)
            {
                LivingEntity t = le.Target;
                t.Hit(le, finisher, balance, damage);
            }
        }

        public bool IsBeingKnockedBack()
        {
            return animations.Any() ? animations.Peek().Peek().Name == "knockback" || animations.Peek().Peek().Name == "slideback" : false;
        }

        public bool IsDead()
        {
            return health <= 0;
        }

        public static void KnockBack(IDescription description)
        {
            LivingEntity le = description as LivingEntity;
            if (le == null)
            {
                return;
            }

            double direction = Math.Atan2(le.Y - le.knockbackFrom.Y, le.X - le.knockbackFrom.X);
            le.ChangeCoordsDelta(Math.Cos(direction) * 4, Math.Sin(direction) * 4);
            le.DrawOffsetX = 0;
            le.DrawOffsetY = 0;
        }

        public static void SlideBack(IDescription description)
        {
            LivingEntity le = description as LivingEntity;
            if (le == null)
            {
                return;
            }

            double direction = Math.Atan2(le.Y - le.knockbackFrom.Y, le.X - le.knockbackFrom.X);
            le.ChangeCoordsDelta(Math.Cos(direction) * 4, Math.Sin(direction) * 4);
            le.DrawOffsetX = 0;
            le.DrawOffsetY = 0;
        }

        public static void GetUp(IDescription description)
        {
            LivingEntity le = description as LivingEntity;
            if (le == null)
            {
                return;
            }

            le.balance = 100;
            le.DrawOffsetX = 0;
            le.DrawOffsetY = 0;
        }

        public static double AnimationFrame(LivingEntity le, double start, double cutoff)
        {
            double ticksLeft = le.animations.Peek().Peek().TicksLeft();
            double duration = le.animations.Peek().Peek().Duration;

            return start + (duration - ticksLeft) * 1.0 / duration * (cutoff - start);
        }

        public static void AnimationDistance(LivingEntity le, double start, double end, Func<double, double, double> distFunc, double scale)
        {
            double t = AnimationFrame(le, start, end);
            double dist = distFunc(t, scale);
            double angle = Math.Atan2(le.target.Y - le.Y, le.target.X - le.X);
            le.DrawOffsetX = Math.Cos(angle) * dist;
            le.DrawOffsetY = Math.Sin(angle) * dist;
        }
    }
}