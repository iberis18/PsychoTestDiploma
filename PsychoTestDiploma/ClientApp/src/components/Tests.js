import React, { Component } from 'react';
import { Button, Input, Modal, ModalHeader, ModalBody, ModalFooter, Form, FormGroup, Label, FormText, Row, Col, Alert } from 'reactstrap';
import { CustomPagination } from './Pagination';


export class Tests extends Component {
    static displayName = Tests.name;

    constructor(props) {
        super(props);
        this.state = {
            tests: []
        };
        this.getTests = this.getTests.bind(this);
    }

    componentDidMount() {
        this.getTests("/api/tests/view");
    }

    async getTests(url) {
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
            this.setState({ tests: data });
        }
        else {
            console.log("Error: ", response.status);
        }
    }

    render() {
        return (
            <div>
                <br/>
                <Row>
                    <Col xs="8"><h2>Список тестов:</h2></Col>
                    <Col xs="2"><ModalImportTest getTests={this.getTests}/></Col>
                </Row>
                <hr />
                <div>
                    {
                        this.state.tests.map((test) => {
                            return <Test test={test} key={test.id} getTests={this.getTests}/>
                        })
                    }
                </div>
            </div>
        );
    }
}


class Test extends React.Component {
    static displayName = Test.name;
    constructor(props) {
        super(props);
        this.state = {};
        this.remove = this.remove.bind(this);
    }
    
    async remove() {
        if (window.confirm("Вы уверены что хотите удалить этот тест?")) {
            const token = sessionStorage.getItem('tokenKey');
            var response = await fetch("/api/tests/" + this.props.test.id, {
                method: "DELETE",
                headers: {
                    "Authorization": "Bearer " + token
                }
            });
            if (response.ok !== true) {
                console.log("Error: ", response.status);
            }
            else {
                this.props.getTests("/api/tests/view");
            }
        }
    }

    render() {
        return (
            <div>
                <Row>
                    <Col xs="8">{this.props.test.name}</Col>
                    <Col xs="2"><Button color='danger' className="col-12"  outline onClick={this.remove}>Удалить</Button></Col>
                </Row>
                <br/>
            </div>
        );
    }
}


class ModalImportTest extends React.Component {
    static displayName = ModalImportTest.name;
    constructor(props) {
        super(props);
        this.state = {
            modal: false,
            files: [],
            normFiles: [],
            images: [],
            successAlertVisible: false,
            dangerAlertVisible: false,
            dangerAlertText: ""
        };

        this.toggle = this.toggle.bind(this);
        this.onSubmit = this.onSubmit.bind(this);
        this.uploadFile = this.uploadFile.bind(this);
        this.uploadNormFile = this.uploadNormFile.bind(this);
        this.uploadImages = this.uploadImages.bind(this);
        this.onChangeSuccessAlert = this.onChangeSuccessAlert.bind(this);
        this.onChangeDangerAlert = this.onChangeDangerAlert.bind(this);
    }
    componentDidMount() {
    }
    toggle() {
        this.setState({
            modal: !this.state.modal,
            dangerAlertText: "",
            dangerAlertVisible: false,
            successAlertVisible: false,
            files: [],
            normFiles: [],
            images: []
        });
        this.props.getTests("/api/tests/view");
    }
    onChangeSuccessAlert(value) {
        this.setState({ successAlertVisible: value });
    }
    onChangeDangerAlert(value) {
        this.setState({ dangerAlertVisible: value });
    }

    async onSubmit(e) {
        e.preventDefault();
        this.setState({dangerAlertText: ""}, async () => {
            var formData = new FormData();
            formData.append('testFile', this.state.files[0]);
            formData.append('normFile', this.state.normFiles[0]);
            for (var i = 0; i < this.state.images.length; i++) {
                formData.append('images', this.state.images[i]);
            }

            const token = sessionStorage.getItem('tokenKey');
            var response = await fetch("/api/tests/importTests", {
                method: "POST",
                headers: {
                    "Authorization": "Bearer " + token
                },
                body: formData
            });
            if (response.ok !== true) {
                console.log("Error: ", response.status);
                if (response.status === 500)
                    {
                        this.setState({dangerAlertText: "Файлы не были сохранены, проверьте их правильность!", normFiles: [], files: []});
                        this.onChangeDangerAlert(true);
                    }
                var data = await response.json();
                this.setState({dangerAlertText: data.errorText});
                this.onChangeSuccessAlert(false);
                this.onChangeDangerAlert(true);
            }
            else {
                this.onChangeDangerAlert(false);
                this.onChangeSuccessAlert(true);
            }
        });
    }

    uploadFile(e) {
        this.setState({ files: e.target.files });
    }
    uploadNormFile(e) {
        this.setState({ normFiles: e.target.files });
    }
    uploadImages(e) {
        this.setState({ images: e.target.files });
    }

    render() {
        return (
            <div>
                <Button color="info" className="col-12"  onClick={this.toggle}>Импортировать</Button>
                <Modal isOpen={this.state.modal}>
                    <Form onSubmit={(e) => { this.onSubmit(e) }} encType="multipart/form-data">
                        <ModalHeader toggle={this.toggle}>Импорт теста</ModalHeader>
                        <Alert color="success" isOpen={this.state.successAlertVisible} toggle={() => { this.onChangeSuccessAlert(false) }} fade={false}>Файлы успешно сохранены!</Alert >
                        <Alert color="danger" isOpen={this.state.dangerAlertVisible} toggle={() => { this.onChangeDangerAlert(false) }} fade={false}>{this.state.dangerAlertText}</Alert >
                        <ModalBody>
                            <FormGroup>
                                <Label for="file">Тест:</Label>
                                <Input type="file" name="file" accept=".xml" id="file" required onChange={this.uploadFile} />
                                <FormText color="muted">
                                    Прикрепите файл тестa в формате xml.
                                </FormText>
                            </FormGroup>
                            <FormGroup>
                                <Label for="normFile">Норма:</Label>
                                <Input type="file" name="normFile" accept=".xml" id="normFile" required onChange={this.uploadNormFile} />
                                <FormText color="muted">
                                    Прикрепите файл норм в формате xml.
                                </FormText>
                            </FormGroup>
                            <FormGroup>
                                <Label for="images">Изображения:</Label>
                                <Input type="file" name="images" accept="image/*" id="images" multiple onChange={this.uploadImages} />
                                <FormText color="muted">
                                    Прикрепите изображения.
                                </FormText>
                            </FormGroup>
                        </ModalBody>
                        <ModalFooter>
                            <Button color="danger" onClick={this.toggle}>Закрыть</Button>
                            <input type="submit" value="Сохранить" className="btn btn-info" />
                        </ModalFooter>
                    </Form>
                </Modal>
            </div>
        );
    }
}


