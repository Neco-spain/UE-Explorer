﻿#if DECOMPILE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UELib;
using UELib.Core;

namespace UELib.Core
{
	public partial class UFunction : UStruct
	{
		/// <summary>
		/// Decompiles this object into a text format of:
		/// 
		///	[FLAGS] function NAME([VARIABLES]) [const]
		///	{
		///		[LOCALS]
		///		
		///		[CODE]
		/// }
		/// </summary>
		/// <returns></returns>
		public override string Decompile()
		{	
			string Code = String.Empty;
			// Don't decompile statements for native functions, because native functions are definitly empty!
			/*if( !HasFunctionFlag( Flags.FunctionFlags.Native ) )
			{*/
				try
				{
					Code = FormatCode();
				}
				catch( Exception e )
				{
					Code = e.Message;
				}
			//}
			return FormatHeader() + (String.IsNullOrEmpty( Code ) ? ";" : "\r\n" + Code);
		}

		private string FormatFlags()
		{
			string output = String.Empty;
			bool normalFunction = true;

			if( HasFunctionFlag( Flags.FunctionFlags.Private ) )
			{
				output += "private ";
			}
			else if( HasFunctionFlag( Flags.FunctionFlags.Protected ) )
			{
				output += "protected ";
			}

			if( Package.Version >= UnrealPackage.VDLLBind && HasFunctionFlag( Flags.FunctionFlags.DLLImport ) )
			{
				output += "dllimport ";
			}

			if( Package.Version > 180 && HasFunctionFlag( Flags.FunctionFlags.Net ) )
			{
				if( HasFunctionFlag( Flags.FunctionFlags.NetReliable ) )
				{
					output += "reliable ";
				}
				else
				{
					output += "unreliable ";
				}

				if( HasFunctionFlag( Flags.FunctionFlags.NetClient ) )
				{
					output += "client ";
				}

				if( HasFunctionFlag( Flags.FunctionFlags.NetServer ) )
				{
					output += "server ";
				}
			}

			if( HasFunctionFlag( Flags.FunctionFlags.Native ) )
			{
				output += iNative > 0 ? FormatNative() + "(" + iNative + ") " : FormatNative() + " ";
			}

			if( HasFunctionFlag( Flags.FunctionFlags.Static ) )
			{
				output += "static ";
			}

			if( HasFunctionFlag( Flags.FunctionFlags.Final ) )
			{
				output += "final ";
			}

			if( HasFunctionFlag( Flags.FunctionFlags.NoExport ) )
			{
				output += "noexport ";
			}

			if( HasFunctionFlag( Flags.FunctionFlags.K2Pure ) )
			{
				output += "k2pure ";
			}

			if( HasFunctionFlag( Flags.FunctionFlags.Invariant ) )
			{
				output += "invariant ";
			}

			if( HasFunctionFlag( Flags.FunctionFlags.Iterator ) )
			{
				output += "iterator ";
			}

			if( HasFunctionFlag( Flags.FunctionFlags.Latent ) )
			{
				output += "latent ";
			}

			if( HasFunctionFlag( Flags.FunctionFlags.Singular ) )
			{
				output += "singular ";
			}

			if( HasFunctionFlag( Flags.FunctionFlags.Simulated ) )
			{
				output += "simulated ";
			}

			if( HasFunctionFlag( Flags.FunctionFlags.Exec ) )
			{
				output += "exec ";
			}

			if( HasFunctionFlag( Flags.FunctionFlags.Event ) )
			{
				output += "event ";
				normalFunction = false;
			}

			if( HasFunctionFlag( Flags.FunctionFlags.Delegate ) )
			{
				output += "delegate ";
				normalFunction = false;
			}

			if( IsOperator() )
			{
				if( IsPre() )
				{
					output += "preoperator ";
				}
				else if( IsPost() )
				{
					output += "postoperator ";
				}
				else
				{
					output += "operator(" + OperPrecedence + ") ";
				}
				normalFunction = false;
			}

			// Don't add function if it's an operator or event or delegate function type!
			if( normalFunction )
			{
	   			output += "function ";
			}
			return output;
		}

		protected override string FormatHeader()
		{
			// static function (string?:) Name(Parms)...
			string output = FormatFlags() + (_ReturnProperty != null ? (_ReturnProperty.GetFriendlyType() + " ") : String.Empty) 
				+ FriendlyName + FormatParms();
			if( HasFunctionFlag( Flags.FunctionFlags.Const ) )
			{
				output += " const";
			}
			return output;
		}

		private string FormatParms()
		{
			string parms = "(";
			foreach( UProperty parm in _ChildParams	)
			{
				parms += parm.Decompile() + (parm != _ChildParams.Last() ? ", " : String.Empty);
			}
			return parms + ")";
		}

		private string FormatLocals()
		{
			int numParms = 0;
			string output = String.Empty;
			string lastType = String.Empty;
			for( var i = 0; i < _ChildLocals.Count; ++ i )
			{
				string curType = _ChildLocals[i].GetFriendlyType();
				string nextType = ((i + 1) < _ChildLocals.Count ? _ChildLocals[i + 1].GetFriendlyType() : String.Empty);

				// If previous is the same as the one now then format the params as one line until another type is reached
				if( curType == lastType )
				{				
					output += _ChildLocals[i].Name + 
					(
						curType == nextType 
						? ((numParms >= 5 && numParms % 5 == 0) 
						  	? ",\r\n\t" + UDecompiler.Tabs 
						  	: ", "
						  ) 
						: ";\r\n"
					); 	
					++ numParms;
				}
				else
				{
					output += (numParms >= 5 ? "\r\n" : String.Empty) + UDecompiler.Tabs + "local " + _ChildLocals[i].Decompile() + 
					(
						(nextType != curType || String.IsNullOrEmpty( nextType ) ) 
						? ";\r\n" 
						: ", "
					);
					numParms = 1;
				}
				lastType = curType;
			}
			return output + (output.Length != 0 ? "\r\n" : String.Empty);
		}

		private string FormatCode()
		{
			UDecompiler.AddTabs( 1 );
			string locals = FormatLocals();
			string code = String.Empty;
			try
			{
				code = ByteCodeManager != null ? ByteCodeManager.Decompile() : String.Empty;
			}
			catch( Exception e )
			{
				code = e.Message;
			}
			finally
			{
				UDecompiler.RemoveTabs( 1 );
			}

			// Empty function!
			if( String.IsNullOrEmpty( locals ) && String.IsNullOrEmpty( code ) )
			{
				return String.Empty;
			}

			return UDecompiler.Tabs + "{\r\n" + 
				locals + 
				code + 
			"\r\n" + UDecompiler.Tabs + "}"; 
		}
	}
}
#endif
                                                                                                           