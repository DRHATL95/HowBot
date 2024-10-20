﻿using Discord;
using Howbot.Core.Models;
using Lavalink4NET.Integrations.Lavasrc;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;

namespace Howbot.Core.Interfaces;

public interface IEmbedService
{
  void Initialize();

  IEmbed CreateEmbed(EmbedOptions embedOptions);

  IEmbed CreateNowPlayingEmbed(ExtendedLavalinkTrack lavalinkTrack, IUser user, TrackPosition? trackPosition,
    float volume);

  IEmbed CreateNowPlayingEmbed(ExtendedLavalinkTrack lavalinkTrack);

  IEmbed GenerateMusicNextTrackEmbed(ITrackQueue queue);

  IEmbed GenerateMusicCurrentQueueEmbed(ITrackQueue queue);

  IEmbed CreateTrackAddedToQueueEmbed(ExtendedLavalinkTrack lavalinkTrack, IUser user);
}
