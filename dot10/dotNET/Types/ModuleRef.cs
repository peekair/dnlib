﻿using System;

namespace dot10.dotNET.Types {
	/// <summary>
	/// A high-level representation of a row in the ModuleRef table
	/// </summary>
	public abstract class ModuleRef : IHasCustomAttribute, IMemberRefParent, IResolutionScope {
		/// <summary>
		/// The row id in its table
		/// </summary>
		protected uint rid;

		/// <inheritdoc/>
		public MDToken MDToken {
			get { return new MDToken(Table.ModuleRef, rid); }
		}

		/// <inheritdoc/>
		public int HasCustomAttributeTag {
			get { return 12; }
		}

		/// <inheritdoc/>
		public int MemberRefParentTag {
			get { return 2; }
		}

		/// <inheritdoc/>
		public int ResolutionScopeTag {
			get { return 1; }
		}

		/// <summary>
		/// From column ModuleRef.Name
		/// </summary>
		public abstract UTF8String Name { get; set; }

		/// <inheritdoc/>
		public override string ToString() {
			return Name.String;
		}
	}

	/// <summary>
	/// A ModuleRef row created by the user and not present in the original .NET file
	/// </summary>
	public class ModuleRefUser : ModuleRef {
		UTF8String name;

		/// <inheritdoc/>
		public override UTF8String Name {
			get { return name; }
			set { name = value; }
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public ModuleRefUser()
			: this(UTF8String.Empty) {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Module name</param>
		public ModuleRefUser(string name)
			: this(new UTF8String(name)) {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Module name</param>
		public ModuleRefUser(UTF8String name) {
			this.name = name;
		}
	}

	/// <summary>
	/// Created from a row in the ModuleRef table
	/// </summary>
	sealed class ModuleRefMD : ModuleRef {
		/// <summary>The module where this instance is located</summary>
		ModuleDefMD readerModule;
		/// <summary>The raw table row. It's null until <see cref="InitializeRawRow"/> is called</summary>
		RawModuleRefRow rawRow;

		UserValue<UTF8String> name;

		/// <inheritdoc/>
		public override UTF8String Name {
			get { return name.Value; }
			set { name.Value = value; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="readerModule">The module which contains this <c>ModuleRef</c> row</param>
		/// <param name="rid">Row ID</param>
		/// <exception cref="ArgumentNullException">If <paramref name="readerModule"/> is <c>null</c></exception>
		/// <exception cref="ArgumentException">If <paramref name="rid"/> is <c>0</c> or &gt; <c>0x00FFFFFF</c></exception>
		public ModuleRefMD(ModuleDefMD readerModule, uint rid) {
#if DEBUG
			if (readerModule == null)
				throw new ArgumentNullException("readerModule");
			if (rid == 0 || rid > 0x00FFFFFF)
				throw new ArgumentException("rid");
			if (readerModule.TablesStream.Get(Table.ModuleRef).Rows < rid)
				throw new BadImageFormatException(string.Format("ModuleRef rid {0} does not exist", rid));
#endif
			this.rid = rid;
			this.readerModule = readerModule;
			Initialize();
		}

		void Initialize() {
			name.ReadOriginalValue = () => {
				InitializeRawRow();
				return readerModule.StringsStream.Read(rawRow.Name);
			};
		}

		void InitializeRawRow() {
			if (rawRow != null)
				return;
			rawRow = readerModule.TablesStream.ReadModuleRefRow(rid);
		}
	}
}
