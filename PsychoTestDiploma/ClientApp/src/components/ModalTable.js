import React, { Component} from 'react';
import { Button, Form, Modal, ModalHeader, ModalBody, ModalFooter, Table } from 'reactstrap';
import '../custom.css';

export default class ModalTable extends React.Component {
    static displayName = ModalTable.name;
    constructor(props) {
        super(props);
        this.state = {
            modal: false
        };
        this.toggle = this.toggle.bind(this);
    }

    toggle() {
        this.setState({
            modal: !this.state.modal
        });
    }

    shouldComponentUpdate(nextProps, nextState) {
        if (nextProps.results.length === 0)
            return false;
        else {
            var flag = false;
            for (var i = 0; i < nextProps.results.length - 1; i++)
                if (nextProps.results[i].test != nextProps.results[i + 1].test)
                    flag = true;
            return !flag;
        } 
    }

    render() {
        var scalesId = [];
        this.props.results.map((result) => {
            result.scales.map((scale) => {
                var s = scalesId.find(x => x.id === scale.idTestScale);
                if (s === undefined && scale.scores != null && scale.scores !== undefined)
                    scalesId.push({ id: scale.idTestScale, name: scale.name });
            });
        });

        return (
            <div>
                <Button color="info" className="col-12" outline onClick={this.toggle}>Таблица</Button>
                <Modal size="lg" isOpen={this.state.modal} scrollable={true}>
                    <ModalHeader toggle={this.toggle}>Сравнение</ModalHeader>
                    <ModalBody>
                        <Table hover bordered size="sm">
                            <thead>
                                <tr>
                                    <th></th>
                                {
                                    this.props.results.map((result, index) => {
                                        return (<th key={index}>{result.date}</th>);
                                    })
                                }
                                </tr>
                            </thead>

                            <tbody>
                                {
                                    scalesId.map((scale, i) => {
                                        return (
                                            <tr key={i}>
                                                <th>{scale.name}</th>
                                                {
                                                    this.props.results.map((result, j) => {
                                                        return (<td key={j}><Cell patientScales={result.scales} idScale={scale.id} /></td>);
                                                    })
                                                }
                                            </tr>
                                        );
                                    })
                                }
                            </tbody>
                        </Table> 
                    </ModalBody>
                    <ModalFooter>
                        <Button color="info" onClick={() => this.toggle()}>Закрыть</Button>
                    </ModalFooter>
                </Modal>
            </div>
        );
    }
}

const Cell = (props) => {
    var s = props.patientScales.find(x => x.idTestScale === props.idScale)
    if (s !== undefined)
        return (<>{s.scores}</>)
    else 
        return (<></>);
};