TelegramBotClient instance:
- handles everything
- has all methods
- started with token as param

StartReceiving:
- Uses threadPool to invoke handlers
- Stopped by cancellation token

Markup:
- ???? some inhouse shit

Menu layout:
- messages use markdown (Bot.types.replymarkups, very weird)

