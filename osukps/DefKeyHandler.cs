namespace osukps {
	class DefKeyHandler : IKeyHandler {

		private int keyCode;

		public DefKeyHandler(int keyCode) {
			this.keyCode = keyCode;
		}

		public byte Handle() {
			return (byte) (KeyState.IsPressed(keyCode) ? 1 : 0);
		}

	}
}
