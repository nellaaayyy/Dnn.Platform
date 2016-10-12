import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import { Tab, Tabs, TabList, TabPanel } from "react-tabs";
import {
    pagination as PaginationActions,
    security as SecurityActions
} from "../../actions";
import SearchBox from "dnn-search-box";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

class ScannerCheckPanelBody extends Component {
    constructor() {
        super();
    }

    renderFileHeader() {
        const fileTableFields = [
            { "name": resx.get("FileName"), "id": "FileName" },
            { "name": resx.get("LastWriteTime"), "id": "LastWriteTime" }
        ];
        const {props} = this;
        let tableHeaders = fileTableFields.map((field) => {
            let className = "scannerCheckHeader scannerCheckHeader-" + field.id;
            return <div className={className}>
                <span>{field.name}</span>
            </div>;
        });

        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedFileList() {
        const {props} = this;
        return props.searchResults.FoundInFiles.map((term, index) => {
            return (
                <div className="scannerCheckItem">
                    <div className="label-name">
                        <div className="label-wrapper">
                            <span>{term.FileName}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-date">
                        <div className="label-wrapper">
                            <span>{term.LastWriteTime}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

    renderDatabaseHeader() {
        const fileTableFields = [
            { "name": resx.get("DatabaseInstance"), "id": "DatabaseInstance" },
            { "name": resx.get("DatabaseValue"), "id": "DatabaseValue" }
        ];
        const {props} = this;
        let tableHeaders = fileTableFields.map((field) => {
            let className = "scannerCheckHeader scannerCheckHeader-" + field.id;
            return <div className={className}>
                <span>{field.name}</span>
            </div>;
        });

        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedDatabaseList() {
        const {props} = this;
        return props.searchResults.FoundInDatabase.map((term, index) => {
            return (
                <div className="scannerCheckItem">
                    <div className="label-columnname">
                        <div className="label-wrapper">
                            <span>{term.ColumnName}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-columnvalue">
                        <div className="label-wrapper">
                            <span>{term.ColumnValue}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

    renderRiskFilesHeader() {
        const fileTableFields = [
            { "name": resx.get("FileName"), "id": "FileName" },
            { "name": resx.get("LastWriteTime"), "id": "LastWriteTime" }
        ];
        const {props} = this;
        let tableHeaders = fileTableFields.map((field) => {
            let className = "scannerCheckHeader scannerCheckHeader-" + field.id;
            return <div className={className}>
                <span>{field.name}</span>
            </div>;
        });

        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderRiskFilesList(risk) {
        const {props} = this;
        let list = props.modifiedFiles.HighRiskFiles;
        if (risk === "low") {
            list = props.modifiedFiles.LowRiskFiles;
        }
        return list.map((term, index) => {
            return (
                <div className="scannerCheckItem">
                    <div className="label-name">
                        <div className="label-wrapper">
                            <span>{term.FilePath}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-date">
                        <div className="label-wrapper">
                            <span>{term.LastWriteTime}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

    renderPortalSettingsHeader() {
        const fileTableFields = [
            { "name": resx.get("PortalId"), "id": "PortalId" },
            { "name": resx.get("SettingName"), "id": "SettingName" },
            { "name": resx.get("SettingValue"), "id": "SettingValue" },
            { "name": resx.get("UserId"), "id": "UserId" },
            { "name": resx.get("LastWriteTime"), "id": "LastWriteTime" }
        ];
        const {props} = this;
        let tableHeaders = fileTableFields.map((field) => {
            let className = "scannerCheckHeader scannerCheckHeader-" + field.id;
            return <div className={className}>
                <span>{field.name}</span>
            </div>;
        });

        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedPortalSettingsList() {
        const {props} = this;
        return props.modifiedSettings.PortalSettings.map((term, index) => {
            return (
                <div className="scannerCheckItem">
                    <div className="label-id">
                        <div className="label-wrapper">
                            <span>{term.PortalId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-name">
                        <div className="label-wrapper">
                            <span>{term.SettingName}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-value">
                        <div className="label-wrapper">
                            <span>{term.SettingValue}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-user">
                        <div className="label-wrapper">
                            <span>{term.LastModifiedByUserId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-date">
                        <div className="label-wrapper">
                            <span>{term.LastModifiedOnDate}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

    renderHostSettingsHeader() {
        const fileTableFields = [
            { "name": resx.get("SettingName"), "id": "SettingName" },
            { "name": resx.get("SettingValue"), "id": "SettingValue" },
            { "name": resx.get("UserId"), "id": "UserId" },
            { "name": resx.get("LastWriteTime"), "id": "LastWriteTime" }
        ];
        const {props} = this;
        let tableHeaders = fileTableFields.map((field) => {
            let className = "scannerCheckHeader scannerCheckHeader-" + field.id;
            return <div className={className}>
                <span>{field.name}</span>
            </div>;
        });

        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedHostSettingsList() {
        const {props} = this;
        return props.modifiedSettings.PortalSettings.map((term, index) => {
            return (
                <div className="scannerCheckItem">
                    <div className="label-name">
                        <div className="label-wrapper">
                            <span>{term.SettingName}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-value">
                        <div className="label-wrapper">
                            <span>{term.SettingValue}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-user">
                        <div className="label-wrapper">
                            <span>{term.LastModifiedByUserId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-date">
                        <div className="label-wrapper">
                            <span>{term.LastModifiedOnDate}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

    renderTabSettingsHeader() {
        const fileTableFields = [
            { "name": resx.get("TabId"), "id": "TabId" },
            { "name": resx.get("PortalId"), "id": "PortalId" },
            { "name": resx.get("SettingName"), "id": "SettingName" },
            { "name": resx.get("SettingValue"), "id": "SettingValue" },
            { "name": resx.get("UserId"), "id": "UserId" },
            { "name": resx.get("LastWriteTime"), "id": "LastWriteTime" }
        ];
        const {props} = this;
        let tableHeaders = fileTableFields.map((field) => {
            let className = "scannerCheckHeader scannerCheckHeader-" + field.id;
            return <div className={className}>
                <span>{field.name}</span>
            </div>;
        });

        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedTabSettingsList() {
        const {props} = this;
        return props.modifiedSettings.TabSettings.map((term, index) => {
            return (
                <div className="scannerCheckItem">
                    <div className="label-tab">
                        <div className="label-wrapper">
                            <span>{term.TabId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-id">
                        <div className="label-wrapper">
                            <span>{term.PortalId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-name">
                        <div className="label-wrapper">
                            <span>{term.SettingName}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-value">
                        <div className="label-wrapper">
                            <span>{term.SettingValue}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-user">
                        <div className="label-wrapper">
                            <span>{term.LastModifiedByUserId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-date">
                        <div className="label-wrapper">
                            <span>{term.LastModifiedOnDate}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

    renderModuleSettingsHeader() {
        const fileTableFields = [
            { "name": resx.get("Type"), "id": "Type" },
            { "name": resx.get("ModuleId"), "id": "ModuleId" },
            { "name": resx.get("PortalId"), "id": "PortalId" },
            { "name": resx.get("SettingName"), "id": "SettingName" },
            { "name": resx.get("SettingValue"), "id": "SettingValue" },
            { "name": resx.get("UserId"), "id": "UserId" },
            { "name": resx.get("LastWriteTime"), "id": "LastWriteTime" }
        ];
        const {props} = this;
        let tableHeaders = fileTableFields.map((field) => {
            let className = "scannerCheckHeader scannerCheckHeader-" + field.id;
            return <div className={className}>
                <span>{field.name}</span>
            </div>;
        });

        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedModuleSettingsList() {
        const {props} = this;
        return props.modifiedSettings.ModuleSettings.map((term, index) => {
            return (
                <div className="scannerCheckItem">
                    <div className="label-type">
                        <div className="label-wrapper">
                            <span>{term.Type}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-module">
                        <div className="label-wrapper">
                            <span>{term.ModuleId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-id">
                        <div className="label-wrapper">
                            <span>{term.PortalId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-name">
                        <div className="label-wrapper">
                            <span>{term.SettingName}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-value">
                        <div className="label-wrapper">
                            <span>{term.SettingValue}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-user">
                        <div className="label-wrapper">
                            <span>{term.LastModifiedByUserId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-date">
                        <div className="label-wrapper">
                            <span>{term.LastModifiedOnDate}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

    onKeywordChanged(keyword) {
        const {props, state} = this;

        if (keyword === "" && props.scannerCheckKeyword !== "") {
            keyword = props.scannerCheckKeyword;
        }

        if (keyword === props.scannerCheckKeyword) {
            props.dispatch(SecurityActions.updatefileSystemAndDatabaseActiveTab("search"));
            return;
        }

        if (keyword && keyword !== "") {
            props.dispatch(SecurityActions.searchFileSystemAndDatabase({ term: keyword }, (data) => {
            }));
        }
        else {
            props.dispatch(SecurityActions.clearFileSystemAndDatabaseSearch());
        }
        props.dispatch(SecurityActions.updatefileSystemAndDatabaseSearchKeyword(keyword));
    }

    getModifiedSettings() {
        const {props, state} = this;
        if (props.scannerCheckActiveTab === "settings") {
            return;
        }

        if (props.modifiedSettings) {
            props.dispatch(SecurityActions.updatefileSystemAndDatabaseActiveTab("settings"));
            return;
        }
        props.dispatch(SecurityActions.getLastModifiedSettings((data) => {
        }));
    }

    getModifiedFiles() {
        const {props, state} = this;
        if (props.scannerCheckActiveTab === "files") {
            return;
        }

        if (props.modifiedFiles) {
            props.dispatch(SecurityActions.updatefileSystemAndDatabaseActiveTab("files"));
            return;
        }
        props.dispatch(SecurityActions.getLastModifiedFiles((data) => {
        }));
    }

    showSearchResults() {
        const {props, state} = this;
        if (props.searchResults && props.scannerCheckActiveTab === "search") {
            return true;
        }
        return false;
    }

    showModifiedSettings() {
        const {props, state} = this;
        if (props.modifiedSettings && props.scannerCheckActiveTab === "settings") {
            return true;
        }
        return false;
    }

    showModifiedFiles() {
        const {props, state} = this;
        if (props.modifiedFiles && props.scannerCheckActiveTab === "files") {
            return true;
        }
        return false;
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <div className={styles.scannerCheckResults}>
                <div className="scannercheck-topbar">
                    <div className={props.scannerCheckActiveTab === "search" ? "search-filter-active" : "search-filter"}>
                        <SearchBox
                            placeholder={resx.get("SearchPlaceHolder") }
                            initialValue={props.scannerCheckKeyword && props.scannerCheckKeyword.length > 0 ? props.scannerCheckKeyword : null}
                            onSearch={this.onKeywordChanged.bind(this) }
                            maxLength={50} />
                        <div className="clear"></div>
                    </div>
                    <div
                        className={this.showModifiedSettings() ? "settings-filter-active" : "settings-filter"}
                        onClick={this.getModifiedSettings.bind(this) }>
                        {resx.get("ModifiedSettings") }
                    </div>
                    <div
                        className={this.showModifiedFiles() ? "files-filter-active" : "files-filter"}
                        onClick={this.getModifiedFiles.bind(this) }>
                        {resx.get("ModifiedFiles") }
                    </div>
                </div>
                {this.showSearchResults() && props.searchResults.FoundInFiles &&
                    <div>
                        <div className="scannerCheckItems-title">
                            {resx.get("SearchFileSystemResult").replace("{0}", props.searchResults.FoundInFiles.length) }
                        </div>
                        {props.searchResults.FoundInFiles.length > 0 &&
                            <div className="scannerCheckItems">
                                { this.renderFileHeader() }
                                { this.renderedFileList() }
                            </div>
                        }
                    </div>
                }
                {this.showSearchResults() && props.searchResults.FoundInDatabase &&
                    <div>
                        <div className="scannerCheckItems-title">
                            {resx.get("SearchDatabaseResult").replace("{0}", props.searchResults.FoundInDatabase.length) }
                        </div>
                        {props.searchResults.FoundInDatabase.length > 0 &&
                            <div className="scannerCheckItems">
                                { this.renderDatabaseHeader() }
                                { this.renderedDatabaseList() }
                            </div>
                        }
                    </div>
                }
                {this.showModifiedFiles() &&
                    <div>
                        <div className="scannerCheckItems-title">
                            {resx.get("HighRiskFiles") }
                        </div>
                        <div className="scannerCheckItems-riskFiles">
                            { this.renderRiskFilesHeader() }
                            { this.renderRiskFilesList("high") }
                        </div>
                    </div>
                }
                {this.showModifiedFiles() &&
                    <div>
                        <div className="scannerCheckItems-title">
                            {resx.get("LowRiskFiles") }
                        </div>
                        <div className="scannerCheckItems-riskFiles">
                            { this.renderRiskFilesHeader() }
                            { this.renderRiskFilesList("low") }
                        </div>
                    </div>
                }
                {this.showModifiedSettings() &&
                    <div>
                        <div className="scannerCheckItems-title">
                            {resx.get("PortalSettings") }
                        </div>
                        <div className="scannerCheckItems-portalSettings">
                            { this.renderPortalSettingsHeader() }
                            { this.renderedPortalSettingsList() }
                        </div>
                    </div>
                }
                {this.showModifiedSettings() &&
                    <div>
                        <div className="scannerCheckItems-title">
                            {resx.get("HostSettings") }
                        </div>
                        <div className="scannerCheckItems-hostSettings">
                            { this.renderHostSettingsHeader() }
                            { this.renderedHostSettingsList() }
                        </div>
                    </div>
                }
                {this.showModifiedSettings() &&
                    <div>
                        <div className="scannerCheckItems-title">
                            {resx.get("TabSettings") }
                        </div>
                        <div className="scannerCheckItems-tabSettings">
                            { this.renderTabSettingsHeader() }
                            { this.renderedTabSettingsList() }
                        </div>
                    </div>
                }
                {this.showModifiedSettings() &&
                    <div>
                        <div className="scannerCheckItems-title">
                            {resx.get("ModuleSettings") }
                        </div>
                        <div className="scannerCheckItems-moduleSettings">
                            { this.renderModuleSettingsHeader() }
                            { this.renderedModuleSettingsList() }
                        </div>
                    </div>
                }
            </div>
        );
    }
}

ScannerCheckPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    searchResults: PropTypes.object,
    modifiedSettings: PropTypes.object,
    modifiedFiles: PropTypes.object,
    scannerCheckKeyword: PropTypes.string,
    scannerCheckActiveTab: PropTypes.string
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        searchResults: state.security.searchResults,
        modifiedSettings: state.security.modifiedSettings,
        modifiedFiles: state.security.modifiedFiles,
        scannerCheckKeyword: state.security.scannerCheckKeyword,
        scannerCheckActiveTab: state.security.scannerCheckActiveTab
    };
}

export default connect(mapStateToProps)(ScannerCheckPanelBody);