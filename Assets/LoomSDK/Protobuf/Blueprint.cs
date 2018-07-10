// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: blueprint.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
/// <summary>Holder for reflection information generated from blueprint.proto</summary>
public static partial class BlueprintReflection {

  #region Descriptor
  /// <summary>File descriptor for blueprint.proto</summary>
  public static pbr::FileDescriptor Descriptor {
    get { return descriptor; }
  }
  private static pbr::FileDescriptor descriptor;

  static BlueprintReflection() {
    byte[] descriptorData = global::System.Convert.FromBase64String(
        string.Concat(
          "Cg9ibHVlcHJpbnQucHJvdG8iSAoYQmx1ZVByaW50Q3JlYXRlQWNjb3VudFR4",
          "Eg8KB3ZlcnNpb24YASABKAUSDQoFb3duZXIYAiABKAkSDAoEZGF0YRgDIAEo",
          "DCJAChBCbHVlUHJpbnRTdGF0ZVR4Eg8KB3ZlcnNpb24YASABKAUSDQoFb3du",
          "ZXIYAiABKAkSDAoEZGF0YRgDIAEoDCIyChFCbHVlUHJpbnRBcHBTdGF0ZRIP",
          "CgdhZGRyZXNzGAEgASgMEgwKBGJsb2IYAiABKAwiIQoQU3RhdGVRdWVyeVBh",
          "cmFtcxINCgVvd25lchgBIAEoCSIhChBTdGF0ZVF1ZXJ5UmVzdWx0Eg0KBXN0",
          "YXRlGAEgASgMIiYKCE1hcEVudHJ5EgsKA2tleRgBIAEoCRINCgV2YWx1ZRgC",
          "IAEoCWIGcHJvdG8z"));
    descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
        new pbr::FileDescriptor[] { },
        new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
          new pbr::GeneratedClrTypeInfo(typeof(global::BluePrintCreateAccountTx), global::BluePrintCreateAccountTx.Parser, new[]{ "Version", "Owner", "Data" }, null, null, null),
          new pbr::GeneratedClrTypeInfo(typeof(global::BluePrintStateTx), global::BluePrintStateTx.Parser, new[]{ "Version", "Owner", "Data" }, null, null, null),
          new pbr::GeneratedClrTypeInfo(typeof(global::BluePrintAppState), global::BluePrintAppState.Parser, new[]{ "Address", "Blob" }, null, null, null),
          new pbr::GeneratedClrTypeInfo(typeof(global::StateQueryParams), global::StateQueryParams.Parser, new[]{ "Owner" }, null, null, null),
          new pbr::GeneratedClrTypeInfo(typeof(global::StateQueryResult), global::StateQueryResult.Parser, new[]{ "State" }, null, null, null),
          new pbr::GeneratedClrTypeInfo(typeof(global::MapEntry), global::MapEntry.Parser, new[]{ "Key", "Value" }, null, null, null)
        }));
  }
  #endregion

}
#region Messages
public sealed partial class BluePrintCreateAccountTx : pb::IMessage<BluePrintCreateAccountTx> {
  private static readonly pb::MessageParser<BluePrintCreateAccountTx> _parser = new pb::MessageParser<BluePrintCreateAccountTx>(() => new BluePrintCreateAccountTx());
  private pb::UnknownFieldSet _unknownFields;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pb::MessageParser<BluePrintCreateAccountTx> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::BlueprintReflection.Descriptor.MessageTypes[0]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public BluePrintCreateAccountTx() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public BluePrintCreateAccountTx(BluePrintCreateAccountTx other) : this() {
    version_ = other.version_;
    owner_ = other.owner_;
    data_ = other.data_;
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public BluePrintCreateAccountTx Clone() {
    return new BluePrintCreateAccountTx(this);
  }

  /// <summary>Field number for the "version" field.</summary>
  public const int VersionFieldNumber = 1;
  private int version_;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public int Version {
    get { return version_; }
    set {
      version_ = value;
    }
  }

  /// <summary>Field number for the "owner" field.</summary>
  public const int OwnerFieldNumber = 2;
  private string owner_ = "";
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public string Owner {
    get { return owner_; }
    set {
      owner_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  /// <summary>Field number for the "data" field.</summary>
  public const int DataFieldNumber = 3;
  private pb::ByteString data_ = pb::ByteString.Empty;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public pb::ByteString Data {
    get { return data_; }
    set {
      data_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override bool Equals(object other) {
    return Equals(other as BluePrintCreateAccountTx);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public bool Equals(BluePrintCreateAccountTx other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if (Version != other.Version) return false;
    if (Owner != other.Owner) return false;
    if (Data != other.Data) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override int GetHashCode() {
    int hash = 1;
    if (Version != 0) hash ^= Version.GetHashCode();
    if (Owner.Length != 0) hash ^= Owner.GetHashCode();
    if (Data.Length != 0) hash ^= Data.GetHashCode();
    if (_unknownFields != null) {
      hash ^= _unknownFields.GetHashCode();
    }
    return hash;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override string ToString() {
    return pb::JsonFormatter.ToDiagnosticString(this);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void WriteTo(pb::CodedOutputStream output) {
    if (Version != 0) {
      output.WriteRawTag(8);
      output.WriteInt32(Version);
    }
    if (Owner.Length != 0) {
      output.WriteRawTag(18);
      output.WriteString(Owner);
    }
    if (Data.Length != 0) {
      output.WriteRawTag(26);
      output.WriteBytes(Data);
    }
    if (_unknownFields != null) {
      _unknownFields.WriteTo(output);
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public int CalculateSize() {
    int size = 0;
    if (Version != 0) {
      size += 1 + pb::CodedOutputStream.ComputeInt32Size(Version);
    }
    if (Owner.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeStringSize(Owner);
    }
    if (Data.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeBytesSize(Data);
    }
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(BluePrintCreateAccountTx other) {
    if (other == null) {
      return;
    }
    if (other.Version != 0) {
      Version = other.Version;
    }
    if (other.Owner.Length != 0) {
      Owner = other.Owner;
    }
    if (other.Data.Length != 0) {
      Data = other.Data;
    }
    _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(pb::CodedInputStream input) {
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
          break;
        case 8: {
          Version = input.ReadInt32();
          break;
        }
        case 18: {
          Owner = input.ReadString();
          break;
        }
        case 26: {
          Data = input.ReadBytes();
          break;
        }
      }
    }
  }

}

public sealed partial class BluePrintStateTx : pb::IMessage<BluePrintStateTx> {
  private static readonly pb::MessageParser<BluePrintStateTx> _parser = new pb::MessageParser<BluePrintStateTx>(() => new BluePrintStateTx());
  private pb::UnknownFieldSet _unknownFields;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pb::MessageParser<BluePrintStateTx> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::BlueprintReflection.Descriptor.MessageTypes[1]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public BluePrintStateTx() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public BluePrintStateTx(BluePrintStateTx other) : this() {
    version_ = other.version_;
    owner_ = other.owner_;
    data_ = other.data_;
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public BluePrintStateTx Clone() {
    return new BluePrintStateTx(this);
  }

  /// <summary>Field number for the "version" field.</summary>
  public const int VersionFieldNumber = 1;
  private int version_;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public int Version {
    get { return version_; }
    set {
      version_ = value;
    }
  }

  /// <summary>Field number for the "owner" field.</summary>
  public const int OwnerFieldNumber = 2;
  private string owner_ = "";
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public string Owner {
    get { return owner_; }
    set {
      owner_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  /// <summary>Field number for the "data" field.</summary>
  public const int DataFieldNumber = 3;
  private pb::ByteString data_ = pb::ByteString.Empty;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public pb::ByteString Data {
    get { return data_; }
    set {
      data_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override bool Equals(object other) {
    return Equals(other as BluePrintStateTx);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public bool Equals(BluePrintStateTx other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if (Version != other.Version) return false;
    if (Owner != other.Owner) return false;
    if (Data != other.Data) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override int GetHashCode() {
    int hash = 1;
    if (Version != 0) hash ^= Version.GetHashCode();
    if (Owner.Length != 0) hash ^= Owner.GetHashCode();
    if (Data.Length != 0) hash ^= Data.GetHashCode();
    if (_unknownFields != null) {
      hash ^= _unknownFields.GetHashCode();
    }
    return hash;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override string ToString() {
    return pb::JsonFormatter.ToDiagnosticString(this);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void WriteTo(pb::CodedOutputStream output) {
    if (Version != 0) {
      output.WriteRawTag(8);
      output.WriteInt32(Version);
    }
    if (Owner.Length != 0) {
      output.WriteRawTag(18);
      output.WriteString(Owner);
    }
    if (Data.Length != 0) {
      output.WriteRawTag(26);
      output.WriteBytes(Data);
    }
    if (_unknownFields != null) {
      _unknownFields.WriteTo(output);
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public int CalculateSize() {
    int size = 0;
    if (Version != 0) {
      size += 1 + pb::CodedOutputStream.ComputeInt32Size(Version);
    }
    if (Owner.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeStringSize(Owner);
    }
    if (Data.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeBytesSize(Data);
    }
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(BluePrintStateTx other) {
    if (other == null) {
      return;
    }
    if (other.Version != 0) {
      Version = other.Version;
    }
    if (other.Owner.Length != 0) {
      Owner = other.Owner;
    }
    if (other.Data.Length != 0) {
      Data = other.Data;
    }
    _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(pb::CodedInputStream input) {
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
          break;
        case 8: {
          Version = input.ReadInt32();
          break;
        }
        case 18: {
          Owner = input.ReadString();
          break;
        }
        case 26: {
          Data = input.ReadBytes();
          break;
        }
      }
    }
  }

}

public sealed partial class BluePrintAppState : pb::IMessage<BluePrintAppState> {
  private static readonly pb::MessageParser<BluePrintAppState> _parser = new pb::MessageParser<BluePrintAppState>(() => new BluePrintAppState());
  private pb::UnknownFieldSet _unknownFields;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pb::MessageParser<BluePrintAppState> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::BlueprintReflection.Descriptor.MessageTypes[2]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public BluePrintAppState() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public BluePrintAppState(BluePrintAppState other) : this() {
    address_ = other.address_;
    blob_ = other.blob_;
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public BluePrintAppState Clone() {
    return new BluePrintAppState(this);
  }

  /// <summary>Field number for the "address" field.</summary>
  public const int AddressFieldNumber = 1;
  private pb::ByteString address_ = pb::ByteString.Empty;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public pb::ByteString Address {
    get { return address_; }
    set {
      address_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  /// <summary>Field number for the "blob" field.</summary>
  public const int BlobFieldNumber = 2;
  private pb::ByteString blob_ = pb::ByteString.Empty;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public pb::ByteString Blob {
    get { return blob_; }
    set {
      blob_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override bool Equals(object other) {
    return Equals(other as BluePrintAppState);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public bool Equals(BluePrintAppState other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if (Address != other.Address) return false;
    if (Blob != other.Blob) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override int GetHashCode() {
    int hash = 1;
    if (Address.Length != 0) hash ^= Address.GetHashCode();
    if (Blob.Length != 0) hash ^= Blob.GetHashCode();
    if (_unknownFields != null) {
      hash ^= _unknownFields.GetHashCode();
    }
    return hash;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override string ToString() {
    return pb::JsonFormatter.ToDiagnosticString(this);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void WriteTo(pb::CodedOutputStream output) {
    if (Address.Length != 0) {
      output.WriteRawTag(10);
      output.WriteBytes(Address);
    }
    if (Blob.Length != 0) {
      output.WriteRawTag(18);
      output.WriteBytes(Blob);
    }
    if (_unknownFields != null) {
      _unknownFields.WriteTo(output);
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public int CalculateSize() {
    int size = 0;
    if (Address.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeBytesSize(Address);
    }
    if (Blob.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeBytesSize(Blob);
    }
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(BluePrintAppState other) {
    if (other == null) {
      return;
    }
    if (other.Address.Length != 0) {
      Address = other.Address;
    }
    if (other.Blob.Length != 0) {
      Blob = other.Blob;
    }
    _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(pb::CodedInputStream input) {
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
          break;
        case 10: {
          Address = input.ReadBytes();
          break;
        }
        case 18: {
          Blob = input.ReadBytes();
          break;
        }
      }
    }
  }

}

public sealed partial class StateQueryParams : pb::IMessage<StateQueryParams> {
  private static readonly pb::MessageParser<StateQueryParams> _parser = new pb::MessageParser<StateQueryParams>(() => new StateQueryParams());
  private pb::UnknownFieldSet _unknownFields;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pb::MessageParser<StateQueryParams> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::BlueprintReflection.Descriptor.MessageTypes[3]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public StateQueryParams() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public StateQueryParams(StateQueryParams other) : this() {
    owner_ = other.owner_;
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public StateQueryParams Clone() {
    return new StateQueryParams(this);
  }

  /// <summary>Field number for the "owner" field.</summary>
  public const int OwnerFieldNumber = 1;
  private string owner_ = "";
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public string Owner {
    get { return owner_; }
    set {
      owner_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override bool Equals(object other) {
    return Equals(other as StateQueryParams);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public bool Equals(StateQueryParams other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if (Owner != other.Owner) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override int GetHashCode() {
    int hash = 1;
    if (Owner.Length != 0) hash ^= Owner.GetHashCode();
    if (_unknownFields != null) {
      hash ^= _unknownFields.GetHashCode();
    }
    return hash;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override string ToString() {
    return pb::JsonFormatter.ToDiagnosticString(this);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void WriteTo(pb::CodedOutputStream output) {
    if (Owner.Length != 0) {
      output.WriteRawTag(10);
      output.WriteString(Owner);
    }
    if (_unknownFields != null) {
      _unknownFields.WriteTo(output);
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public int CalculateSize() {
    int size = 0;
    if (Owner.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeStringSize(Owner);
    }
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(StateQueryParams other) {
    if (other == null) {
      return;
    }
    if (other.Owner.Length != 0) {
      Owner = other.Owner;
    }
    _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(pb::CodedInputStream input) {
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
          break;
        case 10: {
          Owner = input.ReadString();
          break;
        }
      }
    }
  }

}

public sealed partial class StateQueryResult : pb::IMessage<StateQueryResult> {
  private static readonly pb::MessageParser<StateQueryResult> _parser = new pb::MessageParser<StateQueryResult>(() => new StateQueryResult());
  private pb::UnknownFieldSet _unknownFields;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pb::MessageParser<StateQueryResult> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::BlueprintReflection.Descriptor.MessageTypes[4]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public StateQueryResult() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public StateQueryResult(StateQueryResult other) : this() {
    state_ = other.state_;
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public StateQueryResult Clone() {
    return new StateQueryResult(this);
  }

  /// <summary>Field number for the "state" field.</summary>
  public const int StateFieldNumber = 1;
  private pb::ByteString state_ = pb::ByteString.Empty;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public pb::ByteString State {
    get { return state_; }
    set {
      state_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override bool Equals(object other) {
    return Equals(other as StateQueryResult);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public bool Equals(StateQueryResult other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if (State != other.State) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override int GetHashCode() {
    int hash = 1;
    if (State.Length != 0) hash ^= State.GetHashCode();
    if (_unknownFields != null) {
      hash ^= _unknownFields.GetHashCode();
    }
    return hash;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override string ToString() {
    return pb::JsonFormatter.ToDiagnosticString(this);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void WriteTo(pb::CodedOutputStream output) {
    if (State.Length != 0) {
      output.WriteRawTag(10);
      output.WriteBytes(State);
    }
    if (_unknownFields != null) {
      _unknownFields.WriteTo(output);
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public int CalculateSize() {
    int size = 0;
    if (State.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeBytesSize(State);
    }
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(StateQueryResult other) {
    if (other == null) {
      return;
    }
    if (other.State.Length != 0) {
      State = other.State;
    }
    _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(pb::CodedInputStream input) {
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
          break;
        case 10: {
          State = input.ReadBytes();
          break;
        }
      }
    }
  }

}

public sealed partial class MapEntry : pb::IMessage<MapEntry> {
  private static readonly pb::MessageParser<MapEntry> _parser = new pb::MessageParser<MapEntry>(() => new MapEntry());
  private pb::UnknownFieldSet _unknownFields;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pb::MessageParser<MapEntry> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::BlueprintReflection.Descriptor.MessageTypes[5]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public MapEntry() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public MapEntry(MapEntry other) : this() {
    key_ = other.key_;
    value_ = other.value_;
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public MapEntry Clone() {
    return new MapEntry(this);
  }

  /// <summary>Field number for the "key" field.</summary>
  public const int KeyFieldNumber = 1;
  private string key_ = "";
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public string Key {
    get { return key_; }
    set {
      key_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  /// <summary>Field number for the "value" field.</summary>
  public const int ValueFieldNumber = 2;
  private string value_ = "";
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public string Value {
    get { return value_; }
    set {
      value_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override bool Equals(object other) {
    return Equals(other as MapEntry);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public bool Equals(MapEntry other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if (Key != other.Key) return false;
    if (Value != other.Value) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override int GetHashCode() {
    int hash = 1;
    if (Key.Length != 0) hash ^= Key.GetHashCode();
    if (Value.Length != 0) hash ^= Value.GetHashCode();
    if (_unknownFields != null) {
      hash ^= _unknownFields.GetHashCode();
    }
    return hash;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override string ToString() {
    return pb::JsonFormatter.ToDiagnosticString(this);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void WriteTo(pb::CodedOutputStream output) {
    if (Key.Length != 0) {
      output.WriteRawTag(10);
      output.WriteString(Key);
    }
    if (Value.Length != 0) {
      output.WriteRawTag(18);
      output.WriteString(Value);
    }
    if (_unknownFields != null) {
      _unknownFields.WriteTo(output);
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public int CalculateSize() {
    int size = 0;
    if (Key.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeStringSize(Key);
    }
    if (Value.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeStringSize(Value);
    }
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(MapEntry other) {
    if (other == null) {
      return;
    }
    if (other.Key.Length != 0) {
      Key = other.Key;
    }
    if (other.Value.Length != 0) {
      Value = other.Value;
    }
    _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(pb::CodedInputStream input) {
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
          break;
        case 10: {
          Key = input.ReadString();
          break;
        }
        case 18: {
          Value = input.ReadString();
          break;
        }
      }
    }
  }

}

#endregion


#endregion Designer generated code
