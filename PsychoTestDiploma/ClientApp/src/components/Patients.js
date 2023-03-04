import React, { Component } from 'react';
import { Button, Row, Col, Input, InputGroup, InputGroupAddon } from 'reactstrap';
import ModalPatient from './ModalPatient';
import { useHistory } from "react-router-dom";
import { CustomPagination } from './Pagination';


export class Patients extends Component {
    static displayName = Patients.name;

    constructor(props) {
        super(props);
        this.state = {
            patients: [],
            searchString: "",
            emptyPatient: { name: "", tests: [], id: "", results: [] },
            currentPage: 1,
            urlForPagination: "",
            postfixUrlForPagination: ""
        };
        this.getPatients = this.getPatients.bind(this);
        this.onSearchStringChange = this.onSearchStringChange.bind(this);
        this.onCurrentPageChange = this.onCurrentPageChange.bind(this);
    }

    componentDidMount() {
        this.getPatients("/api/patients/page/1");
        this.setState({ searchString: "", urlForPagination: "api/patients/", postfixUrlForPagination: "" });
    }
    onSearchStringChange(e) {
        this.setState({ searchString: e.target.value });
    }
    onCurrentPageChange(value) {
        this.setState({ currentPage: value });
    }
    async getPatients(url) {
        const token = sessionStorage.getItem('tokenKey');
        var response = await fetch(url, {
            method: "GET",
            headers: {
                "Accept": "application/json",
                "Authorization": "Bearer " + token
            }
        });
        var data = await response.json();
        if (response.ok === true) {
            this.setState({ patients: [] });
            this.setState({ patients: data });
        }
        else {
            console.log("Error: ", response.status);
        }
    }

    render() {
        return (
            <div>
                <h2>Список пациентов:</h2>
                <br />
                <Row>
                    <Col xs="1"><strong>Поиск</strong></Col>
                    <Col xs="5">
                        <InputGroup>
                            <Input value={this.state.searchString} onChange={this.onSearchStringChange} />
                            <InputGroupAddon addonType="append">
                                <Button color="secondary" outline onClick={() => { this.getPatients("api/patients/page/" + this.state.currentPage); this.setState({ searchString: "", urlForPagination: "api/patients/", postfixUrlForPagination: "" }) }}>&#215;</Button>
                            </InputGroupAddon>
                        </InputGroup>
                    </Col>
                    <Col xs="2"><Button color="info" className="col-12" onClick={() => {
                        if (this.state.searchString !== "") {
                            this.getPatients("api/patients/name/page/1/" + this.state.searchString);
                            this.setState({ urlForPagination: "api/patients/name/", postfixUrlForPagination: this.state.searchString, currentPage: 1 });
                        }
                        else {
                            this.getPatients("api/patients/page/1");
                            this.setState({ urlForPagination: "api/patients/", postfixUrlForPagination: "", currentPage: 1 })
                        }
                    }}>Найти</Button></Col>
                    <Col xs="1"></Col>
                    <Col xs="auto"><ModalPatient patient={this.state.emptyPatient} isCreate={true} onClose={this.getPatients} currentPage={this.state.currentPage} url={this.state.urlForPagination + "page/" + this.state.currentPage + "/" + this.state.postfixUrlForPagination} /></Col>
                </Row>
                <br />
                <hr />
                <div>
                    {
                        this.state.patients.map((patient) => {
                            return <Patient patient={patient} getPatients={this.getPatients} key={patient.id} currentPage={this.state.currentPage} url={this.state.urlForPagination + "page/" + this.state.currentPage + "/" + this.state.postfixUrlForPagination} />
                        })
                    }
                </div>
                <br />
                <CustomPagination controllerUrl={this.state.urlForPagination} postfixUrl={this.state.postfixUrlForPagination} getContent={this.getPatients} setCurrentPage={this.onCurrentPageChange} className="col-12" />
            </div>
        );
    }
}

class Patient extends Component {
    static displayName = Patient.name;

    constructor(props) {
        super(props);
        this.state = {
            patient: props.patient
        };
    }

    render() {
        return (
            <div>
                <Row>
                    <Col xs="6">{this.state.patient.name}</Col>
                    <Col xs="2"><ViewPatient patientId={this.state.patient.id} /></Col>
                </Row>
                <br />
            </div>
        );
    }
}

const ViewPatient = (patientId) => {
    let history = useHistory();

    const goToPatient = () => {
        history.push("/patient/" + patientId.patientId);
    };
    return (
        <Button color="info" outline className="col-12" onClick={goToPatient}>Просмотр</Button>
    );
};