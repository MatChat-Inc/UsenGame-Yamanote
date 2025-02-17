using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace USEN.Games.Yamanote
{
	public partial class YamanoteDAO
	{
		public const int VERSION = 1;
		
		public static YamanoteDAO Instance { get; } = new();
		
		public readonly SQLiteConnection db;
		
		public bool NeedsUpdate => db.ExecuteScalar<int>("PRAGMA user_version") < VERSION;

		// Called when the node enters the scene tree for the first time.
		public YamanoteDAO(string databaseName = null)
		{
			var databasePath = Path.Combine(Application.persistentDataPath, "DB", databaseName ?? "yamanote.db");
			var databaseDirectory = Path.GetDirectoryName(databasePath);

			// Create the directory if it doesn't exist.
			if (!Directory.Exists(databaseDirectory))
				Directory.CreateDirectory(databaseDirectory!);

			db = new SQLiteConnection(databasePath);
			db.CreateTable<YamanoteQuestion>();

// #if DEBUG
// 			Test();
// #endif
		}
		
		~YamanoteDAO()
		{
			db.Close();
		}
		
		public void InsertFromJsonList(string json)
		{
			if (string.IsNullOrEmpty(json))
				return;
			
			var questions = JsonConvert.DeserializeObject<List<YamanoteQuestion>>(json);

			// New transaction to add all questions at once.
			db.RunInTransaction(() =>
			{
				foreach (var question in questions)
					db.Insert(question);
			});
		}
		public bool IsEmpty()
		{
			return !db.Table<YamanoteQuestion>().Any();
		}

		public void AddQuestion(YamanoteQuestion question)
		{
			db.Insert(question);
		}

		public List<YamanoteQuestion> GetQuestions(string fromCategory = null)
		{
			var questions = db.Query<YamanoteQuestion>("SELECT * FROM questions");
			if (fromCategory == null)
				return questions.ToList();

			var knowledgeQuestions =
				from question in questions
				where question.Category == fromCategory
				select question;

			return knowledgeQuestions.ToList();
		}

		public List<YamanoteCategory> GetCategories()
		{
			var questions = db.Query<YamanoteQuestion>("SELECT * FROM questions");

			var categories =
				from question in questions
				group question by question.Category into categoryGroup
				select new YamanoteCategory
				{
					Name = categoryGroup.Key,
					Questions = categoryGroup.ToList()
				};

			return categories.ToList();
		}
		
		public bool UpdateQuestion(YamanoteQuestion question)
		{
			return db.Update(question) > 0;
		}
		
		public bool DeleteQuestion(YamanoteQuestion question)
		{
			return db.Delete(question) > 0;
		}
		
		public void UpdateTable(string json)
		{
			var version = db.ExecuteScalar<int>("PRAGMA user_version");
			Debug.Log($"[YamanoteDAO] Database version: {version}");
			
			if (version < VERSION)
			{
				db.Execute("DELETE FROM questions WHERE category != 'オリジナル'");
				InsertFromJsonList(json);
				db.Execute($"PRAGMA user_version = {VERSION}");
				Debug.Log($"[YamanoteDAO] Database updated to version {VERSION}");
			}
			else if (IsEmpty())
				InsertFromJsonList(json);
		}

		private void Test()
		{
			var questions = GetQuestions("知識");

			foreach (var question in questions)
				Debug.Log($"{question.Content} - {question.Category} - {question.Theme} - {question.Difficulty}");

			var categories = GetCategories();

			foreach (var category in categories)
			{
				Debug.Log($"{category.Name} - {category.Questions.Count} questions");
				foreach (var question in category.Questions)
				{
					Debug.Log($"  {question.Content} - {question.Theme} - {question.Difficulty}");
				}
			}
		}
	}
}