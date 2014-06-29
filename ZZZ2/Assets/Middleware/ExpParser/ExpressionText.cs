using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

using ExpEvalTS2;

public class ExpressionText 
{
	static IParser mParser = new ExpParser();
	List<Expression> mExpressions = new List<Expression>();
	string mText;

	struct Expression
	{
		public string mKey;
		public string mFormat;
		public ExpEvaluator mEquation;
	}

	public ExpressionText(string text)
	{
		

		mText = text;

		MatchCollection matches = Regex.Matches(text, "{(.*?):.*?}");


		int index = 0;
		foreach (Match match in matches)
		{
			if (match.Groups.Count > 1)
			{
				Expression tempExpression;

				tempExpression.mKey = "{" + index + "}";
				index++;

				string equationValue = match.Groups[1].Value;

				tempExpression.mFormat = match.Value.Replace(equationValue, "0");
				
				tempExpression.mEquation = new ExpEvaluator(mParser);
				tempExpression.mEquation.SetExpression(equationValue);
				
				mExpressions.Add(tempExpression);

				mText = mText.Replace(match.Value, tempExpression.mKey);
			}
		}
	}

	public void setVariable(string name, float value)
	{
		for (int i = 0; i < mExpressions.Count; i++)
		{
			if (mExpressions[i].mEquation != null)
			{
				mExpressions[i].mEquation[name] = (double)value;
			}
		}
	}

	public string compile(Dictionary<string, float> variables = null)
	{
		if (variables != null)
		{
			foreach (KeyValuePair<string, float> keyPair in variables)
			{
				setVariable(keyPair.Key, keyPair.Value);
			}
		}

		string tempText = mText;

		for (int i = 0; i < mExpressions.Count; i++)
		{
			tempText = tempText.Replace(mExpressions[i].mKey, string.Format(mExpressions[i].mFormat, mExpressions[i].mEquation.Evaluate()));
		}

		return tempText;
	}
}
