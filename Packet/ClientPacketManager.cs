using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new();
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new();
		
	public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }

	public void Register()
	{		
		_onRecv.Add((ushort)MessageId.SEnterGame, MakePacket<S_EnterGame>);
		_handler.Add((ushort)MessageId.SEnterGame, PacketHandler.S_EnterGameHandler);		
		_onRecv.Add((ushort)MessageId.SLeaveGame, MakePacket<S_LeaveGame>);
		_handler.Add((ushort)MessageId.SLeaveGame, PacketHandler.S_LeaveGameHandler);		
		_onRecv.Add((ushort)MessageId.SSpawn, MakePacket<S_Spawn>);
		_handler.Add((ushort)MessageId.SSpawn, PacketHandler.S_SpawnHandler);		
		_onRecv.Add((ushort)MessageId.SSpawnProjectile, MakePacket<S_SpawnProjectile>);
		_handler.Add((ushort)MessageId.SSpawnProjectile, PacketHandler.S_SpawnProjectileHandler);		
		_onRecv.Add((ushort)MessageId.SSpawnEffect, MakePacket<S_SpawnEffect>);
		_handler.Add((ushort)MessageId.SSpawnEffect, PacketHandler.S_SpawnEffectHandler);		
		_onRecv.Add((ushort)MessageId.SDespawn, MakePacket<S_Despawn>);
		_handler.Add((ushort)MessageId.SDespawn, PacketHandler.S_DespawnHandler);		
		_onRecv.Add((ushort)MessageId.SPlayerMove, MakePacket<S_PlayerMove>);
		_handler.Add((ushort)MessageId.SPlayerMove, PacketHandler.S_PlayerMoveHandler);		
		_onRecv.Add((ushort)MessageId.SMove, MakePacket<S_Move>);
		_handler.Add((ushort)MessageId.SMove, PacketHandler.S_MoveHandler);		
		_onRecv.Add((ushort)MessageId.SMoveForwardObject, MakePacket<S_MoveForwardObject>);
		_handler.Add((ushort)MessageId.SMoveForwardObject, PacketHandler.S_MoveForwardObjectHandler);		
		_onRecv.Add((ushort)MessageId.SState, MakePacket<S_State>);
		_handler.Add((ushort)MessageId.SState, PacketHandler.S_StateHandler);		
		_onRecv.Add((ushort)MessageId.SSync, MakePacket<S_Sync>);
		_handler.Add((ushort)MessageId.SSync, PacketHandler.S_SyncHandler);		
		_onRecv.Add((ushort)MessageId.SSetPath, MakePacket<S_SetPath>);
		_handler.Add((ushort)MessageId.SSetPath, PacketHandler.S_SetPathHandler);		
		_onRecv.Add((ushort)MessageId.SSetKnockBack, MakePacket<S_SetKnockBack>);
		_handler.Add((ushort)MessageId.SSetKnockBack, PacketHandler.S_SetKnockBackHandler);		
		_onRecv.Add((ushort)MessageId.SSetDestSkill, MakePacket<S_SetDestSkill>);
		_handler.Add((ushort)MessageId.SSetDestSkill, PacketHandler.S_SetDestSkillHandler);		
		_onRecv.Add((ushort)MessageId.SSetDestResource, MakePacket<S_SetDestResource>);
		_handler.Add((ushort)MessageId.SSetDestResource, PacketHandler.S_SetDestResourceHandler);		
		_onRecv.Add((ushort)MessageId.SSetAnimSpeed, MakePacket<S_SetAnimSpeed>);
		_handler.Add((ushort)MessageId.SSetAnimSpeed, PacketHandler.S_SetAnimSpeedHandler);		
		_onRecv.Add((ushort)MessageId.SSkill, MakePacket<S_Skill>);
		_handler.Add((ushort)MessageId.SSkill, PacketHandler.S_SkillHandler);		
		_onRecv.Add((ushort)MessageId.SSkillUpgrade, MakePacket<S_SkillUpgrade>);
		_handler.Add((ushort)MessageId.SSkillUpgrade, PacketHandler.S_SkillUpgradeHandler);		
		_onRecv.Add((ushort)MessageId.SSkillUpdate, MakePacket<S_SkillUpdate>);
		_handler.Add((ushort)MessageId.SSkillUpdate, PacketHandler.S_SkillUpdateHandler);		
		_onRecv.Add((ushort)MessageId.SPortraitUpgrade, MakePacket<S_PortraitUpgrade>);
		_handler.Add((ushort)MessageId.SPortraitUpgrade, PacketHandler.S_PortraitUpgradeHandler);		
		_onRecv.Add((ushort)MessageId.SUnitUpgrade, MakePacket<S_UnitUpgrade>);
		_handler.Add((ushort)MessageId.SUnitUpgrade, PacketHandler.S_UnitUpgradeHandler);		
		_onRecv.Add((ushort)MessageId.SGetDamage, MakePacket<S_GetDamage>);
		_handler.Add((ushort)MessageId.SGetDamage, PacketHandler.S_GetDamageHandler);		
		_onRecv.Add((ushort)MessageId.SChangeHp, MakePacket<S_ChangeHp>);
		_handler.Add((ushort)MessageId.SChangeHp, PacketHandler.S_ChangeHpHandler);		
		_onRecv.Add((ushort)MessageId.SChangeShield, MakePacket<S_ChangeShield>);
		_handler.Add((ushort)MessageId.SChangeShield, PacketHandler.S_ChangeShieldHandler);		
		_onRecv.Add((ushort)MessageId.SChangeMp, MakePacket<S_ChangeMp>);
		_handler.Add((ushort)MessageId.SChangeMp, PacketHandler.S_ChangeMpHandler);		
		_onRecv.Add((ushort)MessageId.SChangeSpeed, MakePacket<S_ChangeSpeed>);
		_handler.Add((ushort)MessageId.SChangeSpeed, PacketHandler.S_ChangeSpeedHandler);		
		_onRecv.Add((ushort)MessageId.SDie, MakePacket<S_Die>);
		_handler.Add((ushort)MessageId.SDie, PacketHandler.S_DieHandler);		
		_onRecv.Add((ushort)MessageId.SConnected, MakePacket<S_Connected>);
		_handler.Add((ushort)MessageId.SConnected, PacketHandler.S_ConnectedHandler);		
		_onRecv.Add((ushort)MessageId.SUnitSpawnPos, MakePacket<S_UnitSpawnPos>);
		_handler.Add((ushort)MessageId.SUnitSpawnPos, PacketHandler.S_UnitSpawnPosHandler);		
		_onRecv.Add((ushort)MessageId.SGetRanges, MakePacket<S_GetRanges>);
		_handler.Add((ushort)MessageId.SGetRanges, PacketHandler.S_GetRangesHandler);		
		_onRecv.Add((ushort)MessageId.STime, MakePacket<S_Time>);
		_handler.Add((ushort)MessageId.STime, PacketHandler.S_TimeHandler);		
		_onRecv.Add((ushort)MessageId.SSetTextUI, MakePacket<S_SetTextUI>);
		_handler.Add((ushort)MessageId.SSetTextUI, PacketHandler.S_SetTextUIHandler);		
		_onRecv.Add((ushort)MessageId.SRegisterInSlot, MakePacket<S_RegisterInSlot>);
		_handler.Add((ushort)MessageId.SRegisterInSlot, PacketHandler.S_RegisterInSlotHandler);		
		_onRecv.Add((ushort)MessageId.SRegisterMonsterInSlot, MakePacket<S_RegisterMonsterInSlot>);
		_handler.Add((ushort)MessageId.SRegisterMonsterInSlot, PacketHandler.S_RegisterMonsterInSlotHandler);		
		_onRecv.Add((ushort)MessageId.SUpgradeSlot, MakePacket<S_UpgradeSlot>);
		_handler.Add((ushort)MessageId.SUpgradeSlot, PacketHandler.S_UpgradeSlotHandler);		
		_onRecv.Add((ushort)MessageId.SSetUpgradePopup, MakePacket<S_SetUpgradePopup>);
		_handler.Add((ushort)MessageId.SSetUpgradePopup, PacketHandler.S_SetUpgradePopupHandler);		
		_onRecv.Add((ushort)MessageId.SSetUpgradeButtonCost, MakePacket<S_SetUpgradeButtonCost>);
		_handler.Add((ushort)MessageId.SSetUpgradeButtonCost, PacketHandler.S_SetUpgradeButtonCostHandler);		
		_onRecv.Add((ushort)MessageId.SSendWarningInGame, MakePacket<S_SendWarningInGame>);
		_handler.Add((ushort)MessageId.SSendWarningInGame, PacketHandler.S_SendWarningInGameHandler);		
		_onRecv.Add((ushort)MessageId.SShowResultScene, MakePacket<S_ShowResultScene>);
		_handler.Add((ushort)MessageId.SShowResultScene, PacketHandler.S_ShowResultSceneHandler);		
		_onRecv.Add((ushort)MessageId.SShowResultPopup, MakePacket<S_ShowResultPopup>);
		_handler.Add((ushort)MessageId.SShowResultPopup, PacketHandler.S_ShowResultPopupHandler);
	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		if (_onRecv.TryGetValue(id, out var action))
			action.Invoke(session, buffer, id);
	}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		if (CustomHandler != null)
		{
			CustomHandler.Invoke(session, pkt, id);
		}
		else
		{
			if (_handler.TryGetValue(id, out var action))
				action.Invoke(session, pkt);
		}
	}

	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{
		if (_handler.TryGetValue(id, out var action))
			return action;
		return null;
	}
}