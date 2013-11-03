﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;

namespace Aura.Data.Database
{
	public class ItemInfo
	{
		public int Id { get; internal set; }
		public int Version { get; internal set; }

		public string Name { get; internal set; }
		public string KorName { get; internal set; }

		public ItemType Type { get; internal set; }

		public byte Width { get; internal set; }
		public byte Height { get; internal set; }

		public byte ColorMap1 { get; internal set; }
		public byte ColorMap2 { get; internal set; }
		public byte ColorMap3 { get; internal set; }
		public byte ColorMode { get; internal set; }

		public StackType StackType { get; internal set; }
		public ushort StackMax { get; internal set; }
		public int StackItem { get; internal set; }
		public int Price { get; internal set; }
		public int SellingPrice { get; internal set; }
		public int Durability;

		public int Defense { get; internal set; }
		public short Protection { get; internal set; }

		public byte WeaponType { get; internal set; }
		public InstrumentType InstrumentType { get; internal set; }

		public short Range { get; internal set; }
		public ushort AttackMin { get; internal set; }
		public ushort AttackMax { get; internal set; }
		public byte Critical { get; internal set; }
		public byte Balance { get; internal set; }
		public byte AttackSpeed { get; internal set; }
		public byte KnockCount { get; internal set; }

		public string OnUse { get; internal set; }
		public string OnEquip { get; internal set; }
		public string OnUnequip { get; internal set; }
	}

	/// <summary>
	/// Item database, indexed by item id.
	/// </summary>
	public class ItemDb : DatabaseCSVIndexed<int, ItemInfo>
	{
		public ItemInfo Find(string name)
		{
			name = name.ToLower();
			return this.Entries.FirstOrDefault(a => a.Value.Name.ToLower() == name).Value;
		}

		public List<ItemInfo> FindAll(string name)
		{
			name = name.ToLower();
			return this.Entries.FindAll(a => a.Value.Name.ToLower().Contains(name));
		}

		protected override void ReadEntry(CSVEntry entry)
		{
			if (entry.Count < 29)
				throw new FieldCountException(29);

			var info = new ItemInfo();
			info.Id = entry.ReadInt();
			info.Version = entry.ReadInt();

			info.Name = entry.ReadString();
			info.KorName = entry.ReadString();
			info.Type = (ItemType)entry.ReadShort();
			info.StackType = (StackType)entry.ReadShort();
			info.StackMax = entry.ReadUShort();

			if (info.StackMax < 1)
				info.StackMax = 1;

			info.StackItem = entry.ReadInt();

			info.Width = entry.ReadByte();
			info.Height = entry.ReadByte();
			info.ColorMap1 = entry.ReadByte();
			info.ColorMap2 = entry.ReadByte();
			info.ColorMap3 = entry.ReadByte();
			info.Price = entry.ReadInt();
			info.SellingPrice = (info.Id != 2000 ? (int)(info.Price * 0.1f) : 1000);
			info.Durability = entry.ReadInt();
			info.Defense = entry.ReadInt();
			info.Protection = entry.ReadShort();
			info.InstrumentType = (InstrumentType)entry.ReadByte();
			info.WeaponType = entry.ReadByte();
			if (info.WeaponType == 0)
			{
				entry.Skip(7);
			}
			else
			{
				info.Range = entry.ReadShort();
				info.AttackMin = entry.ReadUShort();
				info.AttackMax = entry.ReadUShort();
				info.Critical = entry.ReadByte();
				info.Balance = entry.ReadByte();
				info.AttackSpeed = entry.ReadByte();
				info.KnockCount = entry.ReadByte();
			}

			info.OnUse = entry.ReadString();
			info.OnEquip = entry.ReadString();
			info.OnUnequip = entry.ReadString();

			this.Entries.Add(info.Id, info);
		}
	}

	public enum ItemType : ushort
	{
		Armor = 0,
		Headgear = 1,
		Glove = 2,
		Shoe = 3,
		Book = 4,
		Currency = 5,
		ItemBag = 6,
		Weapon = 7,
		Weapon2H = 8, // 2H, bows, tools, etc
		Weapon2 = 9, // Ineffective Weapons? Signs, etc.
		Instrument = 10,
		Shield = 11,
		Robe = 12,
		Accessory = 13,
		SecondaryWeapon = 14,
		MusicScroll = 15,
		Manual = 16,
		EnchantScroll = 17,
		CollectionBook = 18,
		ShopLicense = 19,
		FaliasTreasure = 20,
		Kiosk = 21,
		StyleArmor = 22,
		StyleHeadgear = 23,
		StyleGlove = 24,
		StyleShoe = 25,
		ComboCard = 27,
		Unknown2 = 28,
		Hair = 100,
		Face = 101,
		Usable = 501,
		Quest = 502,
		Usable2 = 503,
		Unknown1 = 504,
		Sac = 1000,
		Misc = 1001,
	}

	public enum StackType : byte
	{
		None = 0,
		Stackable = 1,
		Sac = 2,
	}

	public enum InstrumentType : byte
	{
		Lute = 0,
		Ukulele = 1,
		Mandolin = 2,
		Whistle = 3,
		Roncadora = 4,
		Flute = 5,
		Chalumeau = 6,

		ToneBottleC = 7,
		ToneBottleD = 8,
		ToneBottleE = 9,
		ToneBottleF = 10,
		ToneBottleG = 11,
		ToneBottleB = 12,
		ToneBottleA = 13,

		Tuba = 18,
		Lyra = 19,
		ElectricGuitar = 20,

		Piano = 21,
		Violin = 22,
		Cello = 23,

		BassDrum = 66,
		Drum = 67,
		Cymbals = 68,

		HandbellC = 69,
		HandbellD = 70,
		HandbellE = 71,
		HandbellF = 72,
		HandbellG = 73,
		HandbellB = 74,
		HandbellA = 75,
		HandbellHighC = 76,

		Xylophone = 77,

		MaleVoice1 = 81,
		MaleVoice2 = 82,
		MaleVoice3 = 83,
		MaleVoice4 = 84,
		FemaleVoice1 = 90,
		FemaleVoice2 = 91,
		FemaleVoice3 = 92,
		FemaleVoice4 = 93,
		FemaleVoice5 = 94,
	}
}
