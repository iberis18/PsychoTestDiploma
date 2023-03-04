import React, { Component } from 'react';
import { Button, Modal, ModalHeader, ModalBody, ModalFooter, Row, Col, Input, Form, FormGroup, Label, InputGroup, InputGroupAddon, Alert, Collapse } from 'reactstrap';
import { useHistory } from "react-router-dom";


export default class ModalPatient extends React.Component {
    static displayName = ModalPatient.name;
    constructor(props) {
        super(props);
        this.state = {
            modal: false,
            tests: [],
            prescribedTests: [],
            availableTests: [],
            name: "",
            addedTest: 0,
            isPrescribedTests: "",
            url: "",
            successAlertVisible: false,
            dangerAlertVisible: false,
            isSave: false
        };

        this.toggle = this.toggle.bind(this);
        this.onSubmit = this.onSubmit.bind(this);
        this.onChangeName = this.onChangeName.bind(this);
        this.onTestChange = this.onTestChange.bind(this);
        this.onChangeCheckbox = this.onChangeCheckbox.bind(this);
        this.onChangeSuccessAlert = this.onChangeSuccessAlert.bind(this);
        this.onChangeDangerAlert = this.onChangeDangerAlert.bind(this);
    }

    componentDidMount() {
        this.getTests();
    }
    onChangeSuccessAlert(value) {
        this.setState({ successAlertVisible: value });
    }
    onChangeDangerAlert(value) {
        this.setState({ dangerAlertVisible: value });
    }
    toggle() {
        this.getTests();
        this.setState({
            modal: !this.state.modal,
            name: "",
            isPrescribedTests: "Тестов пока нет!",
            prescribedTests: [],
            isSave: false,
            successAlertVisible: false,
            dangerAlertVisible: false
        });
        this.props.onClose(this.props.url);
    }
    onChangeName(e) {
        this.setState({ name: e.target.value });
    }
    onTestChange(e) {
        this.setState({ addedTest: e.target.value });
    }
    async getTests() {
        const token = sessionStorage.getItem('tokenKey');
        var response = await fetch("/api/tests/view", {
            method: "GET",
            headers: {
                "Accept": "application/json",
                "Authorization": "Bearer " + token
            }
        });
        var data = await response.json();
        if (response.ok === true) {
            this.setState({ tests: data, availableTests: data });
        }
        else {
            console.log("Error: ", response.status);
        }
    }

    addTest() {
        var tmp = this.state.prescribedTests;
        this.state.tests.map((test) => {
            if (test.id === this.state.addedTest) {
                tmp.push({ name: test.name, id: test.id, isChecked: true });
                this.setState({ prescribedTests: tmp });
                tmp = this.state.availableTests;
                for (var i = 0; i < this.state.availableTests.length; i++) {
                    if (this.state.availableTests[i].id === this.state.addedTest) {
                        if (i + 1 !== this.state.availableTests.length)
                            this.setState({ addedTest: this.state.availableTests[i + 1].id });
                        else if (this.state.availableTests.length === 2)
                            this.setState({ addedTest: this.state.availableTests[0].id });
                        else
                            this.setState({ addedTest: 0 });
                        tmp.splice(i, 1);
                        this.setState({ isPrescribedTests: "" });
                    }
                }
                this.setState({ availableTests: tmp });
            }
        });
    }

    onChangeCheckbox(testId, value) {
        var tmp = this.state.prescribedTests;
        for (var i = 0; i < tmp.length; i++) {
            if (tmp[i].id === testId) {
                tmp[i].isChecked = value;
            }
        }
        this.setState({ prescribedTests: tmp });
    }

    async onSubmit(event) {
        event.preventDefault();
        if (this.state.isSave === false) {
            this.setState({ isSave: true });
            const token = sessionStorage.getItem('tokenKey');
            var tests = [];
            this.state.prescribedTests.map((test) => {
                if (test.isChecked === true)
                    tests.push(test.id);
            });
            var response = await fetch("/api/patients/", {
                method: "POST",
                headers: {
                    "Authorization": "Bearer " + token,
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    name: this.state.name,
                    tests: tests,
                    results: []
                })
            });
            var data = await response.json();
            if (response.ok !== true) {
                console.log("Error: ", response.status);
            }
            else {
                this.onChangeSuccessAlert(true);
                this.setState({ url: data.message, isSave: true });
            }
        }
        else {
            this.onChangeSuccessAlert(false);
            this.onChangeDangerAlert(true);
        }
    }

    render() {
        return (
            <div>
                <Button color="info" onClick={this.toggle}>Добавить пациента</Button>
                <Modal size="lg" isOpen={this.state.modal}>
                    <Form onSubmit={this.onSubmit}>
                        <ModalHeader toggle={() => { this.toggle() }}>Новый пациент</ModalHeader>
                        <Alert color="success" isOpen={this.state.successAlertVisible} toggle={() => { this.onChangeSuccessAlert(false) }} fade={false}>Пациент успешно добавлен!</Alert >
                        <Alert color="danger" isOpen={this.state.dangerAlertVisible} toggle={() => { this.onChangeDangerAlert(false) }} fade={false}>Данный пациент уже добавлен!</Alert >
                        <ModalBody>
                            <FormGroup>
                                <Label for="name">Имя:</Label>
                                <Input id="name" required value={this.state.name} onChange={this.onChangeName} placeholder="ФИО" />
                            </FormGroup>

                            <FormGroup>
                                <Label for="newTest">Назначить тест</Label>
                                <Row>
                                    <Col xs="9">
                                        <Input type="select" name="select" defaultValue={'0'} onChange={this.onTestChange} id="newTest">
                                            <option value="0" disabled>Выберите тест</option>
                                            {
                                                this.state.availableTests.map((test) => {
                                                    return (
                                                        <option key={test.id} value={test.id}>{test.name}</option>
                                                    );
                                                })
                                            }
                                        </Input>
                                    </Col>
                                    <Col xs="auto">
                                        <Button color="info" outline onClick={() => this.addTest()}>Назначить</Button>
                                    </Col>
                                </Row>
                            </FormGroup>

                            <FormGroup>
                                <h5>Назначенные тесты:</h5>
                                <p>{this.state.isPrescribedTests}</p>
                                <div>
                                    {
                                        this.state.prescribedTests.map((test) => {
                                            return (
                                                <FormGroup check key={test.id}>
                                                    <Label check>
                                                        <Input type="checkbox" value={test.id} checked={test.isChecked} onChange={(e) => { this.onChangeCheckbox(test.id, e.target.checked) }} />{test.name}</Label>
                                                </FormGroup>
                                            );
                                        })
                                    }
                                </div>
                            </FormGroup>

                            <br />
                            <Collapse isOpen={this.state.isSave}>
                                <FormGroup>
                                    <Row>
                                        <Col xs="2"><Label for="url">Ссылка для привязки:</Label></Col>
                                        <Col xs="10">
                                            <InputGroup>
                                                <Input readOnly id="url" value={this.state.url} />
                                                <InputGroupAddon addonType="append">
                                                    <Button color="secondary" outline onClick={() => { navigator.clipboard.writeText(this.state.url) }}>Копировать</Button>
                                                </InputGroupAddon>
                                            </InputGroup>
                                        </Col>
                                    </Row>
                                </FormGroup>
                            </Collapse>
                        </ModalBody>
                        <ModalFooter>
                            <Button color="danger" onClick={() => this.toggle()}>Закрыть</Button>
                            <input type="submit" value="Сохранить" className="btn btn-info" />
                        </ModalFooter>
                    </Form>
                </Modal>
            </div>
        );
    }
}
