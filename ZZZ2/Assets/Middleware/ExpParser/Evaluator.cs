/*
 * Class Name : ExpEvaluator.cs
 * Author	  : Neelesh Maurya.    
 * Description: This class implemets the Evaluator class.
 * Purpose    : TS2 Design problem submission.
 * Date		  : Sept 26 2004	 
 * */


using System;

namespace ExpEvalTS2
{
	/// <summary>
	/// Summary description for Evaluator.
	/// </summary>
	/// 
	//Parser Factory

	public class ExpEvaluator: IOperatorImp
	{
		//can be user specific
		private IOperatorImp  _OpInst = null;
		private IParser _exp_parser = null;
		private ExpTreeNode _exp_tree = null;

        public	ExpEvaluator(IOperatorImp _ops)
		{
			//Use IParser to parse it 
			_OpInst = _ops;

			// TODO: Add constructor logic here
		}
		public	ExpEvaluator(IParser _parser)
		{
			//Use IParser to parse it 
			_exp_parser = _parser;

			// TODO: Add constructor logic here
		}
		public	ExpEvaluator(IParser _parser,IOperatorImp _ops)
		{
			//Use IParser to parse it 
			_OpInst = _ops;
			_exp_parser = _parser;
			// TODO: Add constructor logic here
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
				if(_exp_tree != null){
					_exp_tree[sym] = value;
				}else{
					ErrorException Er = new ErrorException("Expression is Invalid"); 
					throw Er;
				}
			}

		}
		public bool SetExpression(string _exp)
		{
			//Clear the Previous Exp
			_exp_parser.Clear();
			//now Parse the Exp
			_exp_tree = _exp_parser.ParseExpression(_exp);
			return true;
		}
		public double Evaluate()
		{
			//parse the Exp
			_exp_tree.ValidateExpTree();
			return _evaluate_exp(_exp_tree);
		}
		private double _evaluate_exp(ExpTreeNode n)
		{
			if(n== null)
			{
				ErrorException Er = new ErrorException("No Expression Tree to Evaluate "); 
				throw Er;
			}
			double x,y;
			if (n._ndType == ExpTreeNode.NodeType.Operator)
			{
				
				x = _evaluate_exp(n._left);
				y = _evaluate_exp(n._right);
				return (_OpInst == null) ? ApplyOp(n._value.ToString(),x,y):_OpInst.ApplyOp(n._value.ToString(),x,y);
			}
			return (System.Convert.ToDouble(n._value)) ;

		}
	}
	
}
