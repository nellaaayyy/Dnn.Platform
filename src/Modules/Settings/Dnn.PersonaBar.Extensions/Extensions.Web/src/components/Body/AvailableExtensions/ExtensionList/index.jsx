import React, {Component, PropTypes } from "react";
import GridCell from "dnn-grid-cell";
import ExtensionHeader from "../common/ExtensionHeader";
import ExtensionDetailRow from "../common/ExtensionDetailRow";
import "./style.less";

class ExtensionList extends Component {
    constructor() {
        super();
    }
    render() {
        const {props, state} = this;

        return (
            <GridCell>
                <ExtensionHeader />
                {props.packages.map((_package) => {
                    return <ExtensionDetailRow
                        _package={_package}
                        onDownload={props.onDownload.bind(this)}
                        />;
                }) }
            </GridCell>
        );
    }
}

ExtensionList.propTypes = {
    label: PropTypes.string,
    packages: PropTypes.array,
    collapsed: PropTypes.bool,
    onChange: PropTypes.func,
    type: PropTypes.string,
    onDownload: PropTypes.func
};


export default ExtensionList;