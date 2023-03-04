import React, { Component, useReducer } from 'react';
import { Button, Row, Col, Input, Form, FormGroup, Label, Alert, InputGroup, InputGroupAddon, Collapse, Modal, ModalHeader, ModalBody, ModalFooter, Table  } from 'reactstrap';
import '../custom.css';
import ModalTable from './ModalTable';

export default class Patient extends React.Component {
    static displayName = Patient.name;
    constructor(props) {
        super(props);
        this.state = {
            patientId: props.match.params.id,
            patientToken: "",
            patient: [],
            name: "",
            allTests: [],
            prescribedTests: [],
            availableTests: [],
            patientResults: [ { scales: [ {name: "", scores: "" } ] }],
            passedTests: [],
            addedTest: 0,
            compareTest: 0,
            arePrescribedTests: "",
            areResults: "",
            isPatient: "",
            alertVisible: false,
            filtering: false
        };

        this.onSubmit = this.onSubmit.bind(this);
        this.onChangeName = this.onChangeName.bind(this);
        this.onTestChange = this.onTestChange.bind(this);
        this.onCompareChange = this.onCompareChange.bind(this);
        this.onCommentChange = this.onCommentChange.bind(this);
        this.onChangeCheckbox = this.onChangeCheckbox.bind(this);
        this.remove = this.remove.bind(this);
        this.onChangeAlert = this.onChangeAlert.bind(this);
        this.onSearchStringChange = this.onSearchStringChange.bind(this);
    }

    componentDidMount() {
        this.getPatient("/api/patients/" + this.state.patientId);
    }
    async getPatient(url) {
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
            this.setState({ patient: data, patientResults: data.results, name: data.name, patientToken: data.token }, () => {  
                this.getTests();
            });
        }
        else {
            this.setState({ isPatient: "Данного пациента не существует!" });
            console.log("Error: ", response.status);
        }
    }
    onChangeName(e) {
        this.setState({ name: e.target.value });
    }
    onTestChange(e) {
        this.setState({ addedTest: e.target.value });
    }
    onCompareChange(e) {
        this.setState({ compareTest: e.target.value });
    }
    onCommentChange(e, resultIndex) {
        var tmp = this.state.patientResults;
        var length = tmp.length;
        tmp[length - resultIndex - 1].comment = e.target.value;
        this.setState({ patient: tmp });
    }
    onChangeAlert(value) {
        this.setState({ alertVisible: value });
    }
    onSearchStringChange(e) {
        this.setState({ searchString: e.target.value });
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
            this.setState({ allTests: data }, () => {
                this.addPrescribedTests();
                this.addNameTestsForResults();
                if (this.state.filtering === false)
                    this.addPassedTests();
            });
        }
        else {
            console.log("Error: ", response.status);
        }
    }

    addTest() {
        var tmp = this.state.prescribedTests;
        this.state.allTests.map((test) => {
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
                        this.setState({ arePrescribedTests: "" });
                    }
                }
                this.setState({ availableTests: tmp });
            }
        });
    }

    addPrescribedTests() {
        this.setState({ availableTests: [], prescribedTests: [] }, () => {
            if (this.state.patient.tests !== null && this.state.patient.tests !== undefined && this.state.patient.tests.length !== 0) {
                this.state.allTests.map((test) => {
                    var flag = false;
                    this.state.patient.tests.map((prescribedTest) => {
                        if (prescribedTest === test.id) {
                            var tmp = this.state.prescribedTests;
                            tmp.push({ name: test.name, id: test.id, isChecked: true });
                            this.setState({ prescribedTests: tmp });
                            flag = true;
                        }
                    });
                    if (flag === false) {
                        var tmp = this.state.availableTests;
                        tmp.push({ name: test.name, id: test.id });
                        this.setState({ availableTests: tmp });
                    }
                });
            }
            else
                this.setState({ availableTests: this.state.allTests, arePrescribedTests: "Тестов пока нет!" });
        });
    }

    addNameTestsForResults() {
        var tmp = this.state.patientResults;
        if (this.state.patientResults !== null && this.state.patientResults !== undefined && this.state.patientResults.length !== 0) {
            this.setState({ areResults: "" });
            for (var i = 0; i < tmp.length; i++)
                this.state.allTests.map((test) => {
                    if (tmp[i].test === test.id)
                        tmp[i].name = test.name;
                });
            this.setState({ patientResults: tmp });
        }
        else
            this.setState({ areResults: "Ни один тест еще не пройден!" });
    }

    addPassedTests() {
        var passedTests = [];
        this.state.patientResults.map((result) => {
            var t = passedTests.find(x => x.id === result.test);
            if (t === undefined) {
                this.state.allTests.map((test) => {
                    if (result.test === test.id)
                        passedTests.push({ name: test.name, id: test.id });
                });
            }
        });
        this.setState({ passedTests: passedTests });
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

    compareTests() {
        this.setState({filtering: true}, () => {
            this.getPatient("/api/patients/results/" + this.state.patientId + "/" + this.state.compareTest);
        });
    }

    async onSubmit(event) {
        event.preventDefault();
        const token = sessionStorage.getItem('tokenKey');
        var tests = [];
        this.state.prescribedTests.map((test) => {
            if (test.isChecked === true)
                tests.push(test.id);
        });

        var response = await fetch("/api/patients/" + this.state.patientId, {
            method: "PUT",
            headers: {
                "Authorization": "Bearer " + token,
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                name: this.state.name,
                id: this.state.patientId,
                tests: tests,
                results: this.state.patientResults
            })
        });

        if (response.ok !== true) {
            console.log("Error: ", response.status);
        }
        else
            this.onChangeAlert(true);
    }

    async remove() {
        if (window.confirm("Вы уверены что хотите удалить этого пациента?")) {
            const token = sessionStorage.getItem('tokenKey');
            var response = await fetch("/api/patients/" + this.state.patient.id, {
                method: "DELETE",
                headers: {
                    "Authorization": "Bearer " + token,
                }
            });
            if (response.ok !== true) {
                console.log("Error: ", response.status);
            }
            else {
                this.props.history.push('/');
            }
        }
    }

    render() {
        if (this.state.isPatient === "")
            return (
                <div>
                    <Alert color="success" className='fixed' style={{top: 5}} isOpen={this.state.alertVisible} toggle={() => { this.onChangeAlert(false) }} fade={false}>
                        Изменения успешно сохранены!
                    </Alert >
                    <Form onSubmit={this.onSubmit}>
                        <h4>Личная информация</h4>
                        <br />
                        <FormGroup>
                            <Row>
                                <Col xs="2"><Label for="name">Имя:</Label></Col>
                                <Col xs="8"><Input id="name" required value={this.state.name} onChange={this.onChangeName} placeholder="ФИО" /></Col>
                            </Row>
                        </FormGroup>

                        <Url patient={this.state.patient} />

                        <hr /><br />
                        <h4>Назначенные теcты</h4>
                        <br />
                        <FormGroup>
                            <Row>
                                <Col xs="2"><Label for="newTest">Назначить тест:</Label></Col>
                                <Col xs="8">
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
                                <Col xs="2">
                                    <Button color="info" className="col-12" outline onClick={() => this.addTest()}>Назначить</Button>
                                </Col>
                            </Row>
                        </FormGroup>

                        <FormGroup>
                            <p>Назначенные теcты:</p>
                            <p>{this.state.arePrescribedTests}</p>
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

                        <hr /><br />
                        <h4>Пройденные тесты</h4>
                        <br />
                        <Row>
                            <Col xs="2"><Label for="selectCompare">Сравнить результаты:</Label></Col>
                            <Col xs="8">
                                <InputGroup>
                                    <Input type="select" name="selectCompare" defaultValue={'0'} onChange={this.onCompareChange} id="selectCompare">
                                        <option value="0" disabled>Выберите тест для сравнения</option>
                                        {
                                            this.state.passedTests.map((test) => {
                                                return (
                                                    <option key={test.id} value={test.id}>{test.name}</option>
                                                );
                                            })
                                        }
                                    </Input>
                                    <InputGroupAddon addonType="append">
                                        <Button color="secondary" outline onClick={() => { this.getPatient("/api/patients/" + this.state.patientId); this.setState({ filtering: false }); }}>&#215;</Button>
                                    </InputGroupAddon>
                                </InputGroup>
                            </Col>
                            <Col xs="2">
                                <Button color="info" className="col-12" outline onClick={() => this.compareTests()}>Сравнить</Button>
                            </Col>
                        </Row>    
                        <br/>
                        <Collapse isOpen={this.state.filtering}>
                            <Row>
                                <Col xs="10"></Col>
                                <Col xs="2">
                                    <ModalTable results={this.state.patientResults.slice(0).reverse()} filtering={this.state.filtering}/>
                                </Col>
                            </Row>
                        </Collapse>


                        <br/>
                        <p>{this.state.areResults}</p>
                        {
                            this.state.patientResults.slice(0).reverse().map((result, index) => {
                                return (
                                    <FormGroup key={index}>
                                        <h5>{result.name}</h5>
                                        <p>Дата прохождения: {result.date}</p>
                                        <ul>
                                            {
                                                result.scales.map((scale, i) => {
                                                    if (scale.scores !== null)
                                                        return (
                                                            <li style={{textAlign: 'justify'}} key={i}>Результат: {scale.scores} — {scale.name}. {scale.interpretation}</li>
                                                        );
                                                    else return(null);
                                                    })
                                            }
                                        </ul>
                                        <Label for="comment">Комментарий:</Label>
                                        <Input type="textarea" name="text" id="comment" value={result.comment} onChange={(e) => { this.onCommentChange(e, index) }} />
                                        <br/>
                                    </FormGroup>
                                );
                            })
                        }
                        <br/>
                        <br />
                        <div className='fixed' style={{bottom: 0}}>
                            <FormGroup>
                                <Row>
                                    <Col xs="4"><Button style={{width: '100%'}} color="danger" onClick={() => this.remove()}>Удалить пациента</Button></Col>
                                    <Col xs="4"></Col>
                                    <Col xs="4"><input  style={{width: '100%'}} type="submit" value="Сохранить изменения" className="btn btn-info" /></Col>
                                </Row>
                            </FormGroup>
                        </div>
                    </Form>
                    <br />
                </div>
            );
        else return (<h1>{this.state.isPatient}</h1>);
    }
}

class Url extends Component {
    constructor(props) {
        super(props);
        this.state = {
            url: ""
        };
        this.generateUrl = this.generateUrl.bind(this);
    }
    componentDidMount() {
        this.getUrl();
    }

    componentDidUpdate(prevProps, prevState) {
        if (prevProps.patient !== this.props.patient) {
            this.getUrl();
        }
    }

    async generateUrl() {
        const token = sessionStorage.getItem('tokenKey');
        var response = await fetch("api/link/generateUrl/" + this.props.patient.id, {
            method: "GET",
            headers: {
                "Accept": "application/json",
                "Authorization": "Bearer " + token
            }
        });
        var data = await response.json();
        if (response.ok === true) {
            this.setState({ url: data });
        }
        else {
            console.log("Error: ", response.status);
        }
    }

    async getUrl() {
        if (this.props.patient.id !== undefined) {
            const token = sessionStorage.getItem('tokenKey');
            var response = await fetch("api/link/getUrl/" + this.props.patient.id, {
                method: "GET",
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + token
                }
            });
            var data = await response.json();
            if (response.ok === true) {
                this.setState({ url: data });
            }
            else {
                console.log("Error: ", response.status);
            }
        }
    }

    render() {
        return (
            <div>
                <FormGroup>
                    <Row>
                        <Col xs="2"><Label for="url">Ссылка для привязки:</Label></Col>
                        <Col xs="8">
                            <InputGroup>
                                <Input readOnly id="url" value={this.state.url} />
                                <InputGroupAddon addonType="append">
                                    <Button color="secondary" outline onClick={() => { navigator.clipboard.writeText(this.state.url) }}>Копировать</Button>
                                </InputGroupAddon>
                            </InputGroup>
                        </Col>
                        <Col xs="2"><Button color='info' className="col-12" outline onClick={() => { this.generateUrl() }}>Новая ссылка</Button></Col>
                    </Row>
                </FormGroup>
            </div>
        );
    }
}