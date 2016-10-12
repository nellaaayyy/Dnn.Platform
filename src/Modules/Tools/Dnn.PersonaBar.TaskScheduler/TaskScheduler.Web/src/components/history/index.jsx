import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import { Tab, Tabs, TabList, TabPanel } from "react-tabs";
import {
    pagination as PaginationActions,
    task as TaskActions
} from "../../actions";
import TaskHistoryItemRow from "./taskHistoryItemRow";
import "./style.less";
import util from "../../utils";
import Pager from "../common/Pager";
import resx from "../../resources";

const svgIcon = require(`!raw!./../svg/history.svg`);

const pageSizeStyle = {
    float: "right",
    margin: "0 60px 0 0",
    width: "175px"
};

let pageSizeOptions = [];
let tableFields = [];

class HistoryPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            taskHistoryList: [],
            scheduleId: -1,
            pageIndex: 0,
            pageSize: 10,
            totalCount: 0
        };
    }

    componentWillMount() {
        const {props, state} = this;
        props.dispatch(TaskActions.getScheduleItemHistory({ scheduleId: props.scheduleId, pageIndex: state.pageIndex, pageSize: state.pageSize }));

        tableFields=[];
        tableFields.push({ "name": resx.get("DescriptionColumn"), "id": "LogNotes" });
        tableFields.push({ "name": resx.get("RanOnServerColumn"), "id": "Server" });
        tableFields.push({ "name": resx.get("DurationColumn"), "id": "ElapsedTime" });
        tableFields.push({ "name": resx.get("SucceededColumn"), "id": "Succeeded" });
        tableFields.push({ "name": resx.get("StartEndColumn"), "id": "StartEnd" });

        pageSizeOptions=[];
        pageSizeOptions.push({ "value": "10", "label": "10 entries per page" });
        pageSizeOptions.push({ "value": "25", "label": "25 entries per page" });
        pageSizeOptions.push({ "value": "50", "label": "50 entries per page" });
        pageSizeOptions.push({ "value": "100", "label": "100 entries per page" });
        pageSizeOptions.push({ "value": "250", "label": "250 entries per page" });
    }

    onEnter(key) {
        const { state } = this;
        alert("You pressed enter! My value is: " + state[key]);
    }

    onPageChange(startIndex) {
        const {props, state} = this;
        let pageIndex = startIndex / state.pageSize;
        this.setState({
            pageIndex: pageIndex,
            pageSize: state.pageSize
        }, () => {
            props.dispatch(TaskActions.getScheduleItemHistory({ scheduleId: props.scheduleId, pageIndex: pageIndex, pageSize: state.pageSize }));
        });
    }

    onPageSizeChange(event) {
        let size = event.value;
        const {props, state} = this;
        this.setState({
            pageIndex: 0,
            pageSize: size
        }, () => {
            props.dispatch(TaskActions.getScheduleItemHistory({ scheduleId: props.scheduleId, pageIndex: 0, pageSize: size }));
        });
    }

    renderedHistoryListHeader() {
        const {props} = this;
        let tableHeaders = tableFields.map((field, index) => {
            let className = "historyHeader historyHeader-" + field.id;
            return <div className={className} key={"historyHeader-" + index}>
                <span>{field.name}</span>
            </div>;
        });

        return <div className="historyHeader-wrapper">{tableHeaders}</div>;
    }

    renderPager() {
        const {props, state} = this;
        return (
            <div className="taskHistoryList-pager">
                <Pager total={props.totalCount}
                    startIndex={state.pageIndex * state.pageSize}
                    pageSize={state.pageSize}
                    onPageChange={this.onPageChange.bind(this) }
                    onPageSizeChange={this.onPageSizeChange.bind(this) } 
                    pageSizeStyle={pageSizeStyle}
                    pageSizeOptions={pageSizeOptions}/>
            </div>
        );
    }

    /* eslint-disable react/no-danger */
    renderedHistoryList() {
        const {props} = this;
        if (props.taskHistoryList) {
            return props.taskHistoryList.map((term, index) => {
                return (
                    <TaskHistoryItemRow
                        friendlyName={term.FriendlyName}
                        logNotes={term.LogNotes}
                        server={term.Server}
                        elapsedTime={term.ElapsedTime}
                        succeeded={term.Succeeded}
                        startDate={term.StartDate}
                        endDate={term.EndDate}
                        nextStart={term.NextStart}
                        key={"taskHistoryItem-" + index}
                        closeOnClick={true}>
                    </TaskHistoryItemRow>
                );
            });
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <div>
                <div>
                    <div className="historyIcon" dangerouslySetInnerHTML={{ __html: svgIcon }}></div>
                    <div className="taskHistoryList-title">{props.title}</div>
                    <div className="taskHistoryList-grid">
                        { this.renderedHistoryListHeader() }
                        { this.renderedHistoryList() }
                    </div>
                    { this.renderPager() }
                </div>
            </div>
        );
    }
}

HistoryPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    taskHistoryList: PropTypes.array,
    scheduleId: PropTypes.number,
    title: PropTypes.string,
    totalCount: PropTypes.number
};

function mapStateToProps(state) {
    return {
        taskHistoryList: state.task.taskHistoryList,
        tabIndex: state.pagination.tabIndex,
        totalCount: state.task.totalCount
    };
}

export default connect(mapStateToProps)(HistoryPanelBody);