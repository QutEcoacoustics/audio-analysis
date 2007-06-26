﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.190
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.CompactFramework.Design.Data, Version 2.0.50727.190.
// 
namespace CFRecorder.QutSensors.Services {
    using System.Diagnostics;
    using System.Web.Services;
    using System.ComponentModel;
    using System.Web.Services.Protocols;
    using System;
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="ServiceSoap", Namespace="http://mquter.qut.edu.au/sensors/")]
    public partial class Service : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        /// <remarks/>
        public Service() {
            this.Url = "http://localhost:4761/QutSensors.WebService/Service.asmx";
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://mquter.qut.edu.au/sensors/AddPhotoReading", RequestNamespace="http://mquter.qut.edu.au/sensors/", ResponseNamespace="http://mquter.qut.edu.au/sensors/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void AddPhotoReading(string sensorGuid, string readingGuid, System.DateTime time, [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] byte[] buffer) {
            this.Invoke("AddPhotoReading", new object[] {
                        sensorGuid,
                        readingGuid,
                        time,
                        buffer});
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginAddPhotoReading(string sensorGuid, string readingGuid, System.DateTime time, byte[] buffer, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("AddPhotoReading", new object[] {
                        sensorGuid,
                        readingGuid,
                        time,
                        buffer}, callback, asyncState);
        }
        
        /// <remarks/>
        public void EndAddPhotoReading(System.IAsyncResult asyncResult) {
            this.EndInvoke(asyncResult);
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://mquter.qut.edu.au/sensors/AddAudioReading", RequestNamespace="http://mquter.qut.edu.au/sensors/", ResponseNamespace="http://mquter.qut.edu.au/sensors/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void AddAudioReading(string sensorGuid, string readingGuid, System.DateTime time, [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] byte[] buffer) {
            this.Invoke("AddAudioReading", new object[] {
                        sensorGuid,
                        readingGuid,
                        time,
                        buffer});
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginAddAudioReading(string sensorGuid, string readingGuid, System.DateTime time, byte[] buffer, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("AddAudioReading", new object[] {
                        sensorGuid,
                        readingGuid,
                        time,
                        buffer}, callback, asyncState);
        }
        
        /// <remarks/>
        public void EndAddAudioReading(System.IAsyncResult asyncResult) {
            this.EndInvoke(asyncResult);
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://mquter.qut.edu.au/sensors/FindSensor", RequestNamespace="http://mquter.qut.edu.au/sensors/", ResponseNamespace="http://mquter.qut.edu.au/sensors/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Sensor FindSensor(string sensorGUID) {
            object[] results = this.Invoke("FindSensor", new object[] {
                        sensorGUID});
            return ((Sensor)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginFindSensor(string sensorGUID, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("FindSensor", new object[] {
                        sensorGUID}, callback, asyncState);
        }
        
        /// <remarks/>
        public Sensor EndFindSensor(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((Sensor)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://mquter.qut.edu.au/sensors/UpdateSensor", RequestNamespace="http://mquter.qut.edu.au/sensors/", ResponseNamespace="http://mquter.qut.edu.au/sensors/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void UpdateSensor(string sensorGUID, string name, string friendlyName, string description) {
            this.Invoke("UpdateSensor", new object[] {
                        sensorGUID,
                        name,
                        friendlyName,
                        description});
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginUpdateSensor(string sensorGUID, string name, string friendlyName, string description, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("UpdateSensor", new object[] {
                        sensorGUID,
                        name,
                        friendlyName,
                        description}, callback, asyncState);
        }
        
        /// <remarks/>
        public void EndUpdateSensor(System.IAsyncResult asyncResult) {
            this.EndInvoke(asyncResult);
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://mquter.qut.edu.au/sensors/AddSensorStatus", RequestNamespace="http://mquter.qut.edu.au/sensors/", ResponseNamespace="http://mquter.qut.edu.au/sensors/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void AddSensorStatus(string sensorGUID, System.DateTime time, byte batteryLevel) {
            this.Invoke("AddSensorStatus", new object[] {
                        sensorGUID,
                        time,
                        batteryLevel});
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginAddSensorStatus(string sensorGUID, System.DateTime time, byte batteryLevel, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("AddSensorStatus", new object[] {
                        sensorGUID,
                        time,
                        batteryLevel}, callback, asyncState);
        }
        
        /// <remarks/>
        public void EndAddSensorStatus(System.IAsyncResult asyncResult) {
            this.EndInvoke(asyncResult);
        }
    }
    
    /// <remarks/>
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://mquter.qut.edu.au/sensors/")]
    public partial class Sensor {
        
        private string nameField;
        
        private string friendlyNameField;
        
        private string descriptionField;
        
        private string longitudeField;
        
        private string latitudeField;
        
        /// <remarks/>
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        public string FriendlyName {
            get {
                return this.friendlyNameField;
            }
            set {
                this.friendlyNameField = value;
            }
        }
        
        /// <remarks/>
        public string Description {
            get {
                return this.descriptionField;
            }
            set {
                this.descriptionField = value;
            }
        }
        
        /// <remarks/>
        public string Longitude {
            get {
                return this.longitudeField;
            }
            set {
                this.longitudeField = value;
            }
        }
        
        /// <remarks/>
        public string Latitude {
            get {
                return this.latitudeField;
            }
            set {
                this.latitudeField = value;
            }
        }
    }
}
