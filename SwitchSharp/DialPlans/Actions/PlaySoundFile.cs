namespace SwitchSharp.DialPlans.Actions
{
	/// <summary>
	/// Play a sound file.
	/// </summary>
	public class PlaySoundFile : IAction
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PlaySoundFile"/> class.
		/// </summary>
		/// <param name="soundFile">The sound file.</param>
		public PlaySoundFile(string soundFile)
		{
			SoundFile = soundFile;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PlaySoundFile"/> class.
		/// </summary>
		/// <param name="phraseId">The phrase id.</param>
        public PlaySoundFile(int phraseId)
		{
			PhraseId = phraseId;
		}

		/// <summary>
		/// Gets or sets sound file (file name) to play.
		/// </summary>
		public string SoundFile { get; set; }

		/// <summary>
		/// Gets or sets phrase id.
		/// </summary>
		public int PhraseId { get; set; }

		#region ISwitchAction Members

		/// <summary>
		/// Gets whether the action is terminating.
		/// </summary>
		/// <value>
		/// <c>true</c> means that no more actions can come after this one.
		/// </value>
		public bool IsTerminating
		{
			get { return false; }
		}

		#endregion
	}
}
