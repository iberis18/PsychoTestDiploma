import React, { Component } from 'react';
import { Button, Modal, ModalHeader, ModalBody, ModalFooter, Input, Form, FormGroup, Label, Collapse, Alert } from 'reactstrap';

export default class ModalUser extends React.Component {
    static displayName = ModalUser.name;
    constructor(props) {
        super(props);
        this.state = {
            modal: false,
            button: "",
            modalHeader: "",
            user: this.props.user,
            isCreate: this.props.isCreate,
            oldName: this.props.user.name,
            oldPassword: this.props.user.password,
            oldLogin: this.props.user.login,
            oldRole: this.props.user.role,
            newPassword: "",
            closeButton: "",
            successAlertVisible: false,
            isSave: false
        };
        this.toggle = this.toggle.bind(this);
        this.onSubmit = this.onSubmit.bind(this);
        this.onChangeName = this.onChangeName.bind(this);
        this.onChangePassword = this.onChangePassword.bind(this);
        this.onChangeNewPassword = this.onChangeNewPassword.bind(this);
        this.onChangeEmail = this.onChangeEmail.bind(this);
        this.onChangeRole = this.onChangeRole.bind(this);
        this.remove = this.remove.bind(this);
        this.onChangeSuccessAlert = this.onChangeSuccessAlert.bind(this);
    }

    componentDidMount() {
        if (this.state.isCreate === true)
            this.setState({ button: "Добавить пользователя", modalHeader: "Новый пользователь", closeButton: "Закрыть" });
        else
            this.setState({ button: "Просмотр", modalHeader: "Информация о пользователе", closeButton: "Удалить" });
    }
    toggle() {
        this.setState({
            modal: !this.state.modal
        });
    }
    onChangeSuccessAlert(value) {
        this.setState({ successAlertVisible: value });
    }
    onChangeName(e) {
        var tmp = this.state.user;
        tmp.name = e.target.value;
        this.setState({ user: tmp });
    }
    onChangePassword(e) {
        var tmp = this.state.user;
        tmp.password = e.target.value;
        this.setState({ user: tmp });
    }
    onChangeNewPassword(e) {
        this.setState({ newPassword: e.target.value });
    }
    onChangeEmail(e) {
        var tmp = this.state.user;
        tmp.login = e.target.value;
        this.setState({ user: tmp });
    }
    onChangeRole(e) {
        var tmp = this.state.user;
        tmp.role = e.target.value;
        this.setState({ user: tmp });
    }

    async onSubmit(event) {
        event.preventDefault();
        const token = sessionStorage.getItem('tokenKey');
        if (this.props.isCreate === false) {
            var response = await fetch("/api/users/" + this.props.user.id, {
                method: "PUT",
                headers: {
                    "Authorization": "Bearer " + token,
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    name: this.state.user.name,
                    id: this.props.user.id,
                    role: this.state.user.role,
                    password: this.state.newPassword,
                    login: this.state.user.login
                })
            });
        }
        else {
            if (this.state.isSave === false) {
                var response = await fetch("/api/users/", {
                    method: "POST",
                    headers: {
                        "Accept": "application/json",
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        name: this.state.user.name,
                        id: this.props.user.id,
                        role: this.state.user.role,
                        password: this.state.user.password,
                        login: this.state.user.login
                    })
                });
            }
        }
        if (response.ok !== true) {
            console.log("Error: ", response.status);
        }
        this.onChangeSuccessAlert(true);
        this.setState({ isSave: true });
    }

    async remove() {
        if (window.confirm("Вы уверены что хотите удалить этого пользователя?")) {
            const token = sessionStorage.getItem('tokenKey');
            var response = await fetch("/api/users/" + this.props.user.id, {
                method: "DELETE",
                headers: {
                    "Authorization": "Bearer " + token,
                }
            });
            if (response.ok !== true) {
                console.log("Error: ", response.status);
            }
            else {
                this.props.onClose(this.props.url);
                this.toggle();
            }
        }
    }
 
    onClose() {
        this.toggle();
        this.setState({
            user: {
                name: this.state.oldName,
                password: this.state.oldPassword,
                login: this.state.oldLogin,
                role: this.state.oldRole
            },
            newPassword: "",
            successAlertVisible: false,
            isSave: false
        });
        this.props.onClose(this.props.url);
    }

    render() {
        return (
            <div>
                <Button color="info" outline={!this.state.isCreate} onClick={this.toggle} className="col-12">{this.state.button}</Button>
                <Modal size="lg" isOpen={this.state.modal}>
                    <Form onSubmit={this.onSubmit}>
                        <ModalHeader toggle={() => { this.onClose() }}>{this.state.modalHeader}</ModalHeader>
                        <Alert color="success" isOpen={this.state.successAlertVisible} toggle={() => { this.onChangeSuccessAlert(false) }} fade={false}>Пользователь успешно сохранен!</Alert >
                        <ModalBody>
                            <FormGroup>
                                <Label for="name">Имя пользователя:</Label>
                                <Input type="text" required name="name" id="name" placeholder="Введите ФИО" value={this.state.user.name} onChange={this.onChangeName} />
                            </FormGroup>
                            <FormGroup>
                                <Label for="email">Email:</Label>
                                <Input type="email" required name="email" id="exampleEmail" placeholder="Введите email" value={this.state.user.login} onChange={this.onChangeEmail} />
                            </FormGroup>
                            <Collapse isOpen={this.props.isCreate}>
                                <FormGroup>
                                    <Label for="password">Пароль:</Label>
                                    <Input type="text" required name="password" id="password" placeholder="Введите пароль" value={this.state.user.password} onChange={this.onChangePassword} />
                                </FormGroup>
                            </Collapse>
                            <FormGroup>
                                <Label for="role">Роль:</Label>
                                <Input type="select" name="role" id="role" onChange={this.onChangeRole} value={this.state.user.role}>
                                    <option value={'user'}>Пользователь</option>
                                    <option value={'admin'}>Администратор</option>
                                </Input>
                            </FormGroup>

                            <Collapse isOpen={!this.props.isCreate}>
                                <FormGroup>
                                    <Label for="passwordChange">Сменить пароль:</Label>
                                    <Input type="text" name="passwordChange" id="passwordChange" placeholder="Введите новый пароль" value={this.state.user.newPassword} onChange={this.onChangeNewPassword} />
                                </FormGroup>
                            </Collapse>
                        </ModalBody>
                        <ModalFooter>
                            <Button color="danger" onClick={() => { if(this.state.isCreate) this.onClose(); else this.remove() }}>{this.state.closeButton}</Button>
                            <input type="submit" value="Сохранить" className="btn btn-info" />
                        </ModalFooter>
                    </Form>
                </Modal>
            </div>
        );
    }
}