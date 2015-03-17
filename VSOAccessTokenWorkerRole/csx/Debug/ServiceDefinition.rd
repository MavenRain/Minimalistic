<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="VSOAccessTokenWorkerRole" generation="1" functional="0" release="0" Id="6eb687b3-791c-49be-98f6-24e6de45d042" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="VSOAccessTokenWorkerRoleGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="AccessTokenWorkerRole:Default" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/LB:AccessTokenWorkerRole:Default" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="AccessTokenWorkerRole:AppId" defaultValue="">
          <maps>
            <mapMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/MapAccessTokenWorkerRole:AppId" />
          </maps>
        </aCS>
        <aCS name="AccessTokenWorkerRole:AppSecret" defaultValue="">
          <maps>
            <mapMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/MapAccessTokenWorkerRole:AppSecret" />
          </maps>
        </aCS>
        <aCS name="AccessTokenWorkerRole:CallbackUrl" defaultValue="">
          <maps>
            <mapMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/MapAccessTokenWorkerRole:CallbackUrl" />
          </maps>
        </aCS>
        <aCS name="AccessTokenWorkerRole:StorageConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/MapAccessTokenWorkerRole:StorageConnectionString" />
          </maps>
        </aCS>
        <aCS name="AccessTokenWorkerRole:TokenRequestUrl" defaultValue="">
          <maps>
            <mapMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/MapAccessTokenWorkerRole:TokenRequestUrl" />
          </maps>
        </aCS>
        <aCS name="AccessTokenWorkerRole:TokenStorageUrl" defaultValue="">
          <maps>
            <mapMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/MapAccessTokenWorkerRole:TokenStorageUrl" />
          </maps>
        </aCS>
        <aCS name="AccessTokenWorkerRoleInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/MapAccessTokenWorkerRoleInstances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:AccessTokenWorkerRole:Default">
          <toPorts>
            <inPortMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/AccessTokenWorkerRole/Default" />
          </toPorts>
        </lBChannel>
      </channels>
      <maps>
        <map name="MapAccessTokenWorkerRole:AppId" kind="Identity">
          <setting>
            <aCSMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/AccessTokenWorkerRole/AppId" />
          </setting>
        </map>
        <map name="MapAccessTokenWorkerRole:AppSecret" kind="Identity">
          <setting>
            <aCSMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/AccessTokenWorkerRole/AppSecret" />
          </setting>
        </map>
        <map name="MapAccessTokenWorkerRole:CallbackUrl" kind="Identity">
          <setting>
            <aCSMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/AccessTokenWorkerRole/CallbackUrl" />
          </setting>
        </map>
        <map name="MapAccessTokenWorkerRole:StorageConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/AccessTokenWorkerRole/StorageConnectionString" />
          </setting>
        </map>
        <map name="MapAccessTokenWorkerRole:TokenRequestUrl" kind="Identity">
          <setting>
            <aCSMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/AccessTokenWorkerRole/TokenRequestUrl" />
          </setting>
        </map>
        <map name="MapAccessTokenWorkerRole:TokenStorageUrl" kind="Identity">
          <setting>
            <aCSMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/AccessTokenWorkerRole/TokenStorageUrl" />
          </setting>
        </map>
        <map name="MapAccessTokenWorkerRoleInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/AccessTokenWorkerRoleInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="AccessTokenWorkerRole" generation="1" functional="0" release="0" software="D:\Users\v-oniobi\Documents\Minimalistic\VSOAccessTokenWorkerRole\csx\Debug\roles\AccessTokenWorkerRole" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaWorkerHost.exe " memIndex="-1" hostingEnvironment="consoleroleadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Default" protocol="http" portRanges="5453" />
            </componentports>
            <settings>
              <aCS name="AppId" defaultValue="" />
              <aCS name="AppSecret" defaultValue="" />
              <aCS name="CallbackUrl" defaultValue="" />
              <aCS name="StorageConnectionString" defaultValue="" />
              <aCS name="TokenRequestUrl" defaultValue="" />
              <aCS name="TokenStorageUrl" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;AccessTokenWorkerRole&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;AccessTokenWorkerRole&quot;&gt;&lt;e name=&quot;Default&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/AccessTokenWorkerRoleInstances" />
            <sCSPolicyUpdateDomainMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/AccessTokenWorkerRoleUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/AccessTokenWorkerRoleFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="AccessTokenWorkerRoleUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="AccessTokenWorkerRoleFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="AccessTokenWorkerRoleInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="b0cb3a46-7880-4664-990e-c834b59cdb7f" ref="Microsoft.RedDog.Contract\ServiceContract\VSOAccessTokenWorkerRoleContract@ServiceDefinition">
      <interfacereferences>
        <interfaceReference Id="062134df-ca09-4615-b784-bbaed53bef1c" ref="Microsoft.RedDog.Contract\Interface\AccessTokenWorkerRole:Default@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/VSOAccessTokenWorkerRole/VSOAccessTokenWorkerRoleGroup/AccessTokenWorkerRole:Default" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>