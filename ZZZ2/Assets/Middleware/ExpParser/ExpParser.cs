/*
 * Class Name : ExpParser.cs
 * Author	  : Neelesh Maurya.    
 * Description: This class implemets the parser for the Expression.
 *				This is an Infix Expression Parser. 	
 * Purpose    : TS2 Design problem submission.
 * Date		  : Sept 26 2004	 
 * */
using System;
using System.Text.RegularExpressions;

namespace ExpEvalTS2
{
	public enum Token { Literal, Symbol, Operator,InValid, Eof};
	//IParser Interface Defnition
	//I have provided the Parser as IParser -- this way the user can leverage 
	//To write his own Expression tree parser-Given that he derives from Iparser
	//then he can pass that to evaluator for parsing -- IParser
	//Try to parser thread safe
	public interface IParser
	{
		//Tyoe 
		//These are the three functions I think are necessary for time being
		//We could have the functions like print Tokens and all Utility functions
		//Error throwing in all should be Similar across the all derived objects
		ExpTreeNode ParseExpression(string exp);
		//This function Should return the Expression in string format
		//In case of the Infix Exp Tree we can not make the Exact Exp
		//if the Exp contained the "(" Operators
		string  ReMakeExpression(ExpTreeNode Tree);
		//This function should return the priority of the supplied operator
		int Priority(string Operator);
		//This function can be used to register new Tokens--
		//INfact i ws thinking of making another Tokeniser class ..But thing looks small
		//Doing here only--
		//Call this function before calling pasre to take this to effect
		bool RegisterToken(string Token, ExpEvalTS2.Token tk_type);

		//The parser should Support this function to clean itself 
		void Clear();

	}
	

	//This is the Implementation of the ExpParser. It parses the Exp using two 
	//Stacks .In case of the two operators having the same priority it make the tree
	//with left to right association....that means while evaluation the computation 
	//would be done from left to right.
	public class ExpParser:IParser
	{
		//protected enum Token { Literal, Symbol, Operator, Eof};
		//protected enum Token { Literal, Symbol, Operator, Eof};
		//Memeber variables all are private
		//Hope they are thread safe
		Regex  reOps;		// Operators regular expression 
		Regex  reLit;		// Litrels regular expression 
		Regex  reSym;		// Symbols regular expression  

		//Type of the Current Token
		Token  current;	
		//Current Sysmboool
		string symbol;		
		string Curr_op;		// current operator
		double  literal;		// current literal
				
		int Curr_idx;			// subExpression Start position
		string _exp;			// Expression to be searched
		TreeStack OpStack;
		TreeStack NodeStack; 
	
		//Constructor 
		public ExpParser()
		{
			//Using the Regular Exp ---- C# Seems better-- feeling good 
			reOps = new Regex(@"^\s*(&&|\|\||<=|>=|==|!=|[=+\-*/^()!<>])");
			reSym = new Regex(@"^\s*([_a-zA-Z]+[_a-zA-Z0-9]*)");
			reLit = new Regex(@"^\s*(-?[0-9]+(\.[0-9]+)?)");

			TreeStack st = new TreeStack(100);  
			TreeStack st1 = new TreeStack(100);  
			OpStack = st;
			NodeStack = st1;

		}
		//Not Implemented 

		public string  ReMakeExpression(ExpTreeNode Tree)
		{
			//Doing nothing right now
			return "";
		}
		//very Important function-=-Self Explainatory		
		private  void PopConnectPush() 
		{
			ExpTreeNode Temp = OpStack.Pop(); //Get Top operator and associate the operands
			//Find here if the Operator is "="  - then dont make the Symbol InVALID--Give some value
			
			
			Temp._right  = NodeStack.Pop(); //Get Left and right from NodeStck
			if(Temp._value.ToString()  == "=")
			{
				ExpTreeNode TmpLeft = NodeStack.Pop();

				//Now this needs to be Symbol with no Sub tree
				if(TmpLeft._ndType != ExpTreeNode.NodeType.Symbol)
				{
						
					ErrorException Er = new ErrorException("Operator = at the wrong Place Left Operand must be lvalue "); 					
					throw Er;

				}
				if(TmpLeft._left ==null ||TmpLeft._right  ==null)
				{
					TmpLeft._value = -1;//SYMBOL TO BE CALCULATED
					Temp._left =TmpLeft;
					NodeStack.Push(Temp); // puch the sub tree on the stack
					return;
				}
				else
				{
					ErrorException Er = new ErrorException("Operator = at the Wrong Place Left Operand must be lvalue"); 					
					throw Er;
				}

			}
			Temp._left = NodeStack.Pop();
			NodeStack.Push(Temp); // puch the sub tree on the stack
		}

		public bool RegisterToken(string Token, ExpEvalTS2.Token tk_type)
		{
			return true;
		}

		//no errors from this function-- very primitive function 
		//This funtion gives the relative priority of the operators
		//This is being used in teh parser. USer of parser can also 
		//view the priority 
		public int Priority(string Operator)
		{
			int Ret_pr; //To be returned
			switch(Operator)
			{
				case "+" :
				case "-":
					Ret_pr = 1;
					break;
				case "*" :
				case "/":
					Ret_pr = 2;
					break;
				case "^":
					Ret_pr = 3;
					break;
				default:
					Ret_pr = 0;
					break;
			}
			return Ret_pr;
		}
		

		//Parse Expression funtion main funtion 
		//Creating the Exp tree from the Infix Expression
		public ExpTreeNode ParseExpression(string exp)
		{
			//Check for the Expresiioon validity
			if(!Isvalid_Exp(exp))
			{
				//Throw an Error Saying Commas are not proper
				ErrorException Er = new ErrorException("Operators ( or ) are not at the right places"); 
				throw Er;
				//Is c# need this return after that -- i dont think so but anyway
				
			}
			//Fill ur _exp Variable will need that
			_exp =  exp;
			 
			NextToken();
			do
			{
				//The errors that might come would be from stack 
				//If that went wrong -- gone case -- What use rwould do
				//No USer specific Errors from this funtion
				switch(current)
				{
					case Token.Literal:
						//CreateLeafNode and push it on the stack
						ExpTreeNode LNode = new ExpTreeNode();
						LNode._value  = literal;
						LNode._ndType = ExpTreeNode.NodeType.Literal;  
						NodeStack.Push(LNode); 
						break;
					case Token.Operator:
						//pop the St 
						ExpTreeNode ONode = new ExpTreeNode();
						ONode._value = Curr_op;
						ONode._ndType = ExpTreeNode.NodeType.Operator;
						if(Curr_op == "(")
						{
							OpStack.Push(ONode); 
							break;
						}
						if(Curr_op == ")")
						{
							while("(" != (string)OpStack.Top._value)
							{
								PopConnectPush();
							}
							OpStack.Pop() ; //Throw away {{
							break;
						}

						if(OpStack.IsEmpty())
						{
							OpStack.Push(ONode); 
						}
						else if((string)OpStack.Top._value == "(")
						{
							OpStack.Push(ONode);
						}
						else if(Priority(OpStack.Top._value.ToString()) < Priority(ONode._value.ToString() ))
						{
							OpStack.Push(ONode);
						}
						else
						{
							while(true)
							{
								PopConnectPush();
								if(OpStack.IsEmpty() == true || (string)OpStack.Top._value == "(" || Priority(OpStack.Top._value.ToString() ) < Priority(ONode._value.ToString() ))
									break;	
							}
							OpStack.Push(ONode);
						}
						break;
					case Token.Symbol: 
						{
							ExpTreeNode SNode = new ExpTreeNode();
							SNode._symbol = symbol;
							SNode._ndType = ExpTreeNode.NodeType.Symbol;  
							NodeStack.Push(SNode); 
						}
						break;
					default:
						break;
				}
				
				
			}while(NextToken() != Token.Eof);

			while(OpStack.IsEmpty() == false)
			{
				PopConnectPush();
			}
			//Get the Top of the Node Stack thats the root
			return NodeStack.Top; 

		}
		//What cleaning i have to do
		public void Clear()
		{
		
			current = Token.InValid;	
			//Current Sysmboool
			symbol = null;		
			Curr_op = null;		// current operator
			literal = 0;		// current literal
				
			Curr_idx =0;			// subExpression Start position
			_exp = null;			// Expression to be searched
			while(OpStack.IsEmpty()!= true)OpStack.Pop(); 
			while(NodeStack.IsEmpty()!= true)NodeStack.Pop(); 
 		
		}


		//Checks just the ( an d( eveness
		private bool Isvalid_Exp(string s)
		{
			//Search the number of the 
			//Firt genration Check
			 string [] Open = s.Split('('); 
			 string [] Closed = s.Split(')'); 
			if(Open.Length == Closed.Length)
                return true;
			else
				return false;
		}
		//Gets the Next Token
		protected Token NextToken()
		{
			Match m;

			m = reSym.Match(_exp);
			if (m.Length != 0)
			{
				symbol = m.Groups[1].Value;
				Curr_idx += m.Length;
				_exp = _exp.Substring(m.Length);
				current = Token.Symbol;
			}
			else
			{
				m = reLit.Match(_exp);
				if (m.Length != 0)
				{
					literal = Double.Parse(m.Groups[1].Value);
					Curr_idx += m.Length;
					_exp = _exp.Substring(m.Length);
					current = Token.Literal;
				}
				else
				{
					m = reOps.Match(_exp);
					if (m.Length != 0)
					{
						Curr_op = m.Groups[1].Value;
						Curr_idx += m.Length;
						_exp = _exp.Substring(m.Length);
						current = Token.Operator;
					}
					else
					{
						current = Token.Eof;
					}					
				}
			}

			return current;
		}
	
	
	}

	//Utility class Stack
	public class TreeStack 
	{
		public bool IsEmpty()
		{
			if(top ==0)
				return true;
			else
				return false;
			
		}
		public TreeStack(int size)
		{
			data = new ExpTreeNode[size];
			top  = 0;
		}

		public void Push(ExpTreeNode f)
		{
			data[top++] = f;			
		}

		public ExpTreeNode Pop()
		{
			if(top >0)
			{
				return data[--top];
			}
			return null;
		}

		public ExpTreeNode Top
		{
			get 
			{
				if(top >0)
				{
					return data[top-1]; 
				}
				else
					return null;
			}
			set { data[top-1] = value; }
		}

		public void Clear()
		{
			top = 0;
		}

		ExpTreeNode [] data;
		int top; 
	}
}
