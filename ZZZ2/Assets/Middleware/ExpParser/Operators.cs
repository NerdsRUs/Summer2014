/*
 * Class Name : IOperatorImp.cs
 * Author	  : Neelesh Maurya.    
 * Description: This class implemets the operator Implementation.
 * Purpose    : TS2 Design problem submission.
 * Date		  : Sept 26 2004	 
 * */

using System;

namespace ExpEvalTS2
{


	//This class be overridden if anyother operator needs to be added 
	//Currently while parsing we accept following as operator @"^\s*(&&|\|\||<=|>=|==|!=|[=+\-*/^()!<>]
	//One can register the Sysmbols using the RegisterToken of IParser funtion 
	public abstract class IOperatorImp//:IOperator---needs to be implemented  
	{

		//This function is currently needed only-- add some mroe funtion 
		public virtual double ApplyOp( string Op,double operand1,double operand2)
		{
			double z =0;
			switch(Op)
			{
				case  "+" : z=operand1+operand2;      break;
				case  "-" : z=operand1-operand2;      break;
				case  "*" : z=operand1*operand2;      break;
				case  "/" : z=operand1/operand2;      break;
				case  "^" : z=System.Math.Pow(operand1,operand2); break;
				case "&&" : z = System.Convert.ToDouble(System.Convert.ToBoolean(operand1) && System.Convert.ToBoolean(operand2));break;
				case "||" : z = System.Convert.ToDouble(System.Convert.ToBoolean(operand1) || System.Convert.ToBoolean(operand2));break;
				case "=": z = operand2; break;
				case ">": z = System.Math.Max(operand1, operand2); break;
				case "<": z = System.Math.Min(operand1, operand2); break;
				default:
				{
					ErrorException Er = new ErrorException("Unknown Operator " + Op); 
					throw Er;

				}
	

			}
			return z;
		}

	}
	
}
