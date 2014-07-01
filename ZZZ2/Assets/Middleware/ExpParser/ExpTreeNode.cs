/*
 * Class Name : ExpTreeNode.cs
 * Author	  : Neelesh Maurya.    
 * Description: This class implemets the Expression Tree Node
 *				Only needed functions has been written. other utility functions 
 *				can be written for printing/remaking the expression.
 * Purpose    : TS2 Design problem Submission.
 * Date		  : Sept 26 2004	 
 * */


using System;


namespace ExpEvalTS2
{
	/// <summary>
	/// Summary description for ExpTreeNode.
	/// </summary>
	/// We could have an Enumrator here to help in Evaluation.. 
	public class ExpTreeNode
	{
		//Type of the Nodes IF its Literal ,Symbol,Operator,Functional,Invalid
		public enum NodeType {Literal,Symbol,Operator,Function,InValid};
        public const object INVALID = null;

		public ExpTreeNode()
		{
			//Constructor
			_value = INVALID;//Default
			_symbol = null;
			_ndType = NodeType.InValid;
			_left = null;
			_right = null;
		}
		//Indexed property for setting the Symbol--
		//Can be done while parsing only..But Ok this way also--Infact better
		//This way the value of the Symbol can be chnaged after the parsing is
		//done even and Expression can be 
		//re-valuated without parsing again.
		public double this[string sym]
		{
			//Get needs to be witten 
			set 
			{
				SetSymbolValue(sym,value);
			}
		}
		// Sets the Symbols if that exists in its Sub Tree
		//Errors I can't see any until unless any major system set 
		//back has happened.
		private void SetSymbolValue(string symb,double val)
		{
			if(_ndType == NodeType.Symbol)
			{
				if(symb == _symbol)
				{
					_value = val;
				}
				
			}
			//Else Traverse down
			if(this._left !=null)
				this._left.SetSymbolValue(symb,val);
			if(this._right !=null)
				this._right.SetSymbolValue(symb,val);
	
		}
		//The Function provides the validity of the Node
		//For Sysmbols it show somw error that the Sysmbols not yet populated
		public bool ValidateExpTree()
		{
			bool ret_val = false;
			bool Left_retval = false; 
			bool right_retval = false;
			
			switch(this._ndType)
			{
				case NodeType.Literal:
					{
						if(this._value == INVALID){
							ErrorException Er = new ErrorException("Literal " + this._value.ToString() + " is at invalid place"); 
							ret_val= false;
							throw Er;
							//return false;
						}
						else
							ret_val = true;
					}
					break;
				case NodeType.Symbol:
					{
						//The Lit Value should not be invalid
						if(this._value == INVALID)
						{
							ErrorException Er = new ErrorException("Unknown Symbol " + this._symbol); 
							ret_val= false;
							throw Er;
							//return false;
						}
						else
							ret_val = true;
					}
					break;
				case NodeType.Operator:
					{
						if(this._left  == null ||this._right == null){
							ErrorException Er = new ErrorException("Operator " + this._value.ToString() + " not at proper Place"); 
							ret_val= false;
							throw Er;
							//return false;
						}else{
							ret_val = true;
						}					
					}
					break;
				case NodeType.Function:
					//To be DOne
					break;
				case NodeType.InValid:
				{
					ErrorException Er = new ErrorException("Operator " + this._value.ToString() + " not at proper Place"); 
					ret_val= false;
					throw Er;
					//return false;
				}

			}
			//now call for the subTrees
			if(this._left !=null)
				Left_retval =this._left.ValidateExpTree(); 
			if(this._right !=null)
				right_retval =this._right.ValidateExpTree(); 

			return Left_retval && right_retval && ret_val;
		}

		//See if given through properties --But guess of no use doing that way
		#region Public Vars

		public object		_value;
		public string		_symbol;
		public NodeType		_ndType;	
		public ExpTreeNode	_left;
		public ExpTreeNode	_right;

		#endregion
				
	}
}
