import React, {PropTypes, Component} from "react";
import { connect } from "react-redux";
import Collapse from "react-collapse";
import Scrollbars from "react-custom-scrollbars";
import SingleLineInput from "dnn-single-line-input";
import MultiLineInput from "dnn-multi-line-input";
import Button from "dnn-button";
import InputGroup from "dnn-input-group";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import "./style.less";
import {
    log as LogActions
} from "../../../actions";
import resx from "../../../resources";
import util from "../../../utils";

class EmailPanel extends Component {
    constructor() {
        super();
        this.state = {
            emailRequest: {
                Email: "",
                Subject: resx.get("LogEntryDefaultSubject"),
                Message: resx.get("LogEntryDefaultMsg"),
                LogIds: []
            },
            error: {
                email: false
            }
        };
    }
    componentWillMount() {
        this.SetErrorState();
    }
    SetErrorState() {
        let {state} = this;
        let {emailRequest} = this.state;
        if (!this.validateEmail(emailRequest.Email)) {
            state.error["email"] = true;
        }
        else {
            state.error["email"] = false;
        }
        state.triedToSubmit = false;
        this.setState({
            state
        });
    }
    onSendEmail(event) {
        event.preventDefault();
        const {props, state} = this;
        const { emailRequest } = this.state;
        emailRequest["LogIds"] = props.logIds;
        this.setState({
            emailRequest,
            triedToSubmit: true
        });
        if (state.error.email) {
            return;
        }
        if (props.logIds.length <= 0) {
            util.utilities.notify(resx.get("SelectException"));
            return;
        }
        props.dispatch(LogActions.emailLogItems(state.emailRequest, (data) => {
            util.utilities.notify(data.ErrorMessage + data.ReturnMessage);
            if (data.Success) {
                this.onCloseEmailPanel();
            }
        }));
    }
    validateEmail(value) {
        const re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(value);
    }
    onEmailValueChange(key, event) {
        const value = event.target.value;
        const { emailRequest } = this.state;
        emailRequest[key] = value;
        this.setState({
            emailRequest
        });
        this.SetErrorState();
    }

    onCloseEmailPanel(event) {
        const {props} = this;
        props.onCloseEmailPanel();
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <div className="collapsible-content-email">
                <Collapse
                    fixedHeight={props.fixedHeight}
                    keepCollapsedContent={props.keepCollapsedContent}
                    isOpened={props.isOpened}>
                    {props.fixedHeight &&
                        <Scrollbars width={props.collapsibleWidth || "100%"} height={props.collapsibleHeight || "100%"} style={props.scrollAreaStyle}>
                            <div>
                                <div className="emailpanel-content-wrapper" style={{ height: "calc(100% - 100px)" }}>
                                    <div className="">
                                        <InputGroup>
                                            <label title={resx.get("plEmailAddress.Help") }>{resx.get("plEmailAddress") }</label>
                                            <SingleLineInputWithError
                                                error={state.error.email && state.triedToSubmit}
                                                errorMessage={resx.get("Email.Message") }
                                                inputStyle={{ marginBottom: "0px" }}
                                                value={state.emailRequest.Email}
                                                onChange={this.onEmailValueChange.bind(this, "Email") }/>
                                        </InputGroup>
                                        <InputGroup>
                                            <label title={resx.get("plSubject.Help") }>{resx.get("plSubject") }</label>
                                            <div>
                                                <SingleLineInput
                                                    value={state.emailRequest.Subject}
                                                    onChange={this.onEmailValueChange.bind(this, "Subject") }
                                                    />
                                            </div>
                                        </InputGroup>
                                        <InputGroup>
                                            <label title={resx.get("SendMessage.Help") }>{resx.get("SendMessage") }</label>
                                            <MultiLineInput
                                                value={state.emailRequest.Message}
                                                onChange={this.onEmailValueChange.bind(this, "Message") }
                                                />
                                        </InputGroup>
                                        <div className="action-buttons">
                                            <Button type="secondary" onClick={this.onCloseEmailPanel.bind(this) }>{resx.get("btnCancel") }</Button>
                                            <Button type="primary" onClick={this.onSendEmail.bind(this) }>{resx.get("btnSend") }</Button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </Scrollbars>
                    }
                    {!props.fixedHeight && props.children}
                </Collapse>
            </div>
        );
    }
}

EmailPanel.PropTypes = {
    label: PropTypes.string,
    fixedHeight: PropTypes.number,
    collapsibleWidth: PropTypes.number,
    collapsibleHeight: PropTypes.number,
    keepCollapsedContent: PropTypes.bool,
    scrollAreaStyle: PropTypes.object,
    children: PropTypes.node,
    isOpened: PropTypes.bool,
    logIds: PropTypes.array,
    onCloseEmailPanel: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        logList: state.log.logList
    };
}

export default connect(mapStateToProps)(EmailPanel);