# Settlement

A game where you try to defeat your mortal enemy.

# About

Settlement was originally created for my Games and Social Justice class at Northeastern. It won the Most Innovative Game award at the 2017 Northeastern Game Showcase.

The game has two parts. The board game and the game bot.

# Creating the Board Game

## Board

The board is an 8 by 8 grid of tiles. Left to right should be labeled A - H. Top to bottom should be labeled 1 - 8. A dark line should be drawn between row 4 and 5 to indicate the half way point. Red side is 1 - 4 and blue side is 5 - 8.

#### You need 2 identical boards.

Here is my board.

![Settlement board](http://i.imgur.com/LgloB17.jpg)

The tiles are 1 inch by 1 inch. Labeling each grid tile helps players when they are playing.

## Settlements

You need two sets of pieces. Settlement pieces should fit into grid squares. My pieces were made of paper and were 3/4s of an inch by 3/4s of an inch.

![Settlement pieces](http://i.imgur.com/lnLibCT.jpg)

## Missiles

You need 6 missile pieces. My pieces were origimi rockets. Instructions to make them are [here](http://www.origami-make.org/origami-rocket-3d.php).

![Settlement missiles](http://i.imgur.com/XcTFdll.jpg)

## Final Setup

You also need a divider so that each player's board is secret. My divider was two pieces of foam board with putty to keep them together.

![Final setup](http://i.imgur.com/INVcAqH.jpg)

## Game Instructions

Game instructions are located in the Game Instructions folder. They are in PDF and DOCX format.

# Setting up the Game Bot

## Secrets.cs

First you'll need to setup Secrets.cs. See Secrets.example.cs for what to populate the file with. Rename it to Secrets.cs and the class to Secrets once you're done.

## Setup

Clone the project into a directory. You will also need to clone my [Slack API wrapper](https://github.com/golf1052/SlackAPI) and [Trexler, a URI helper](https://github.com/golf1052/Trexler). All three projects should be one directory. The structure should look like this.

```
projects
|-- godbot
|-- SlackAPI
|-- Trexler
```

You will need .NET Core in order to run the bot. [Download here](https://www.microsoft.com/net/download/core).

Then run these commands. They need to be run inside the `godbot\godbot` directory.

```
dotnet restore
dotnet run
```

If everything is set up correctly you should see something like this in your terminal.

```
Hosting environment: Production
Content root path: C:\Users\Sanders\OneDrive\Projects\godbot\godbot
Now listening on: http://127.0.0.1:8893
Application started. Press Ctrl+C to shut down.
```

## Twilio

After setting up your Twilio phone number you need to point Twilio to a URL that the bot is setup on. If running locally you can use ngrok to open up your localhost port to the internet. [Download ngrok here](https://ngrok.com/download).

### For localhost with ngrok

```
ngrok http 8893
```

You will now see two URLs that are being forwarded to `localhost:8893`.

### Twilio endpoint

Paste in your URL to the first Messaging webhook and append `/api/godbot`. It should look something like `http://1af951eb.ngrok.io/api/godbot`. Also make sure Twilio will use HTTP POST when connecting to the bot.

You can now test that everything is working by sending `start game [phone number]` to your Twilio number. The number cannot be the number you're sending from. You can setup and use a [Google Voice](https://voice.google.com/) number to test.

## Slack

The Slack bot was originally used for testing. You should be able to use it still. In debug mode the bot outputs all messages to one channel. The channel ID is defined in Client.cs under ChannelId. Change this to the Slack channel you want to use for testing. `golf1052UserId` is the user ID of the person who can start games. Change this to your Slack user ID. `golf1052Dm` is not used in debug mode. You can then say `godbot start game` to start a game in your debug channel. You'll need to enter moves for both players into the channel.

If you want to use the Slack bot like the Twilio bot where the bot messages two channels individually you'll need to make some changes to Client.cs in `Client_MessageReceived()`.
