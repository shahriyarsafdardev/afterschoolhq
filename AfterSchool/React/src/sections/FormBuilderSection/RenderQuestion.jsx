import cs from 'classnames';
import { cloneDeep } from 'lodash';
import React, { useEffect, useState } from 'react';
import {
  DragIndicator as DragIndicatorIcon,
  ArrowDropDownOutlined as ArrowDropDownOutlinedIcon
} from '@material-ui/icons';

import {
  ClickAwayListener,
  useMediaQuery,
  Typography,
  TextField,
  useTheme,
  Button,
  Chip,
  Grid,
  Box
} from '@material-ui/core';

import {
  audienceTypes,
  questionType,
  questionTypes as allQuestionTypes,
  formTypeId
} from 'constants';
import QuestionControlActions from './QuestionControlActions';
import RenderAudienceList from './RenderAudienceList';
import { StyledImage, getMaxLimit } from './helper';
import LabeledCheckBox from './LabeledCheckBox';
import RatingQuestion from './RatingQuestion';
import RenderOptions from './RenderOptions';
import CheckBoxLink from './CheckBoxLink';
import MoodQuestion from './MoodQuestion';
import LockIcon from 'assets/lock.svg';
import DropDown from './DropDown';
import useStyles from './styles';

const RenderQuestion = ({
  index,
  value,
  errors,
  touched,
  formType,
  handleCopy,
  handleDelete,
  setFieldValue,
  dragIndicatorProps,
  handleAddNewQuestion
}) => {
  const theme = useTheme();
  const classes = useStyles();
  const [isOpen, setIsOpen] = useState(false);
  const [question, setQuestion] = useState(value);
  const [initialized, setInitialized] = useState(false);
  const [timeoutConst, setTimeoutConst] = useState(null);
  const [showAudience, setShowAudience] = useState(false);
  const [deleteSectionId, setdeleteSectionId] = useState(null);
  const [showOptionsList, setShowOptionsList] = useState(false);
  const questions = formType === formTypeId.survey ? allQuestionTypes : questionType;
  const { dropdown, radioList, checkBoxList, checkBox, mood, rating } = allQuestionTypes;
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const {
    ID,
    Text,
    Link,
    Options,
    IsLocked,
    QuestionTypeId,
    QuestionSettings,
    QuestionDataTypeId
  } = question;

  const volunteerQS = QuestionSettings[0];
  const maxLimit = getMaxLimit(QuestionTypeId);
  const questionHasOptions = (qTypeId = QuestionTypeId) =>
    [dropdown, radioList, checkBoxList].includes(qTypeId);
  const showOptions =
    ((questionHasOptions() && showOptionsList) || Boolean(touched?.Options && errors?.Options)) &&
    QuestionTypeId !== mood;
  const [options, setOptions] = useState(cloneDeep(Options) || []);
  const customQuestion = QuestionDataTypeId === 0;
  const updateValue = () => {
    if (timeoutConst) {
      clearTimeout(timeoutConst);
    }
    const tempTimeout = setTimeout(() => {
      setFieldValue(index, question);
    }, 1000);
    setTimeoutConst(tempTimeout);
  };

  const questionTypesOptions = Object.keys(questions).map(key => ({
    key: questions[key],
    value: key
  }));

  useEffect(() => {
    setInitialized(true);
  }, []);

  useEffect(() => {
    if (initialized) {
      updateValue();
    }
  }, [question]);

  const getOptions = key => {
    !questionHasOptions(key) && setOptions([]);
    return questionHasOptions(key) ? { Options: options } : { Options: undefined };
  };

  const getLink = (qTypeId = QuestionTypeId) => {
    return { Link: qTypeId !== checkBox ? null : Link };
  };

  const handleDeleteSection = async () => {
    await handleDelete(deleteSectionId);
    handleCloseDialog();
  };

  const handleCloseDialog = () => {
    setdeleteSectionId(null);
    setIsOpen(false);
  };

  const updatedVolunteerSetting = propertyName => {
    const setting = volunteerQS;
    let updatedSetting = { ...setting };
    switch (propertyName) {
      case 'show':
        updatedSetting.Show = !setting.Show;
        updatedSetting.Required = updatedSetting.Show ? setting.Required : false;
        break;
      case 'required':
        updatedSetting.Required = !setting.Required;
        updatedSetting.Show = updatedSetting.Required ? true : setting.Show;
        break;
      default:
        break;
    }
    return updatedSetting;
  };

  return (
    <ClickAwayListener onClickAway={() => setShowOptionsList(false)}>
      <Box className={classes.question} onClick={() => setShowOptionsList(true)}>
        <Grid container className={cs(classes.questionFieldsWrapper)}>
          <Grid item lg={12} sm={12} xs={12} container className={classes.displayFlex}>
            <Grid item lg={1} sm={1} xs={1} className={cs(classes.displayFlex)}>
              <span {...dragIndicatorProps} data-testid={`dragIconForQuestions-${ID}-${index}`}>
                <DragIndicatorIcon style={{ float: 'left', color: '#B2B2B4' }} />
              </span>
              <Typography variant="h6">{index}</Typography>
            </Grid>
            <Grid item lg={8} sm={11} xs={11} className={classes.gridItemHeight}>
              <TextField
                className={classes.formControlClass}
                fullWidth
                size="small"
                variant="outlined"
                value={Text}
                helperText={
                  errors?.Text === 'Text is Required' ? errors?.Text : `${Text?.length}/${maxLimit}`
                }
                error={Text?.length > maxLimit || errors?.Text}
                multiline
                onChange={event => {
                  setQuestion({ ...question, Text: event.target.value });
                }}
                disabled={!customQuestion || IsLocked}
                data-testid={`questionNameField-${ID}-${index}`}
              />
            </Grid>
            <Grid item lg={3} md={4} sm={4} xs={6} className={classes.gridItemHeight}>
              <DropDown
                question={{ ...question, ...{ options: questionTypesOptions } }}
                setFieldValue={key =>
                  setQuestion({
                    ...question,
                    QuestionTypeId: key,
                    ...getOptions(key),
                    ...getLink()
                  })
                }
                disabled={!customQuestion || IsLocked}
                dataTestId={`questionTypeDropDown-${ID}-${index}`}
              />
            </Grid>
            <Grid lg={1}></Grid>
            <>
              {formType !== 3 ? (
                <Grid item lg={11} md={8} sm={6} xs={6} className={cs(classes.audienceWrapper)}>
                  <Grid item lg={2} md={5} sm={12} xs={12} className={classes.positionRelative}>
                    <Button
                      variant="outlined"
                      className={classes.audienceButton}
                      onClick={() => setShowAudience(!showAudience)}
                      disabled={IsLocked}
                      data-testid={`audienceButtonForQuestion-${ID}-${index}`}
                    >
                      Audience
                      <ArrowDropDownOutlinedIcon className={classes.dropDownIcon} />
                    </Button>
                    {showAudience && (
                      <RenderAudienceList
                        setFieldValue={questionSettings =>
                          setQuestion({ ...question, QuestionSettings: questionSettings })
                        }
                        questionSettings={QuestionSettings}
                        setShowAudience={setShowAudience}
                        dataTestId={`audienceListForQuestion-${ID}-${index}`}
                      />
                    )}
                  </Grid>
                  {!isMobile && (
                    <Grid item lg={9} md={6} className={cs(classes.displayFlex, classes.flexWrap)}>
                      {QuestionSettings.map(({ Audience, Show, Required }) => {
                        if (!(Show || Required)) return;

                        return (
                          <Chip
                            style={{ backgroundColor: '#F2F2F5', margin: '1px' }}
                            label={`${audienceTypes[Audience]}${Required ? '*' : ''}`}
                            key={Audience}
                          />
                        );
                      })}
                    </Grid>
                  )}
                </Grid>
              ) : (
                <Grid item lg={3} md={3} sm={4} xs={7} className={cs(classes.audienceWrapper)}>
                  <LabeledCheckBox
                    setFieldValue={() =>
                      setQuestion({
                        ...question,
                        QuestionSettings: [{ ...updatedVolunteerSetting('show') }]
                      })
                    }
                    dataTestId={`showCheckboxOfQuestion-${ID}-${index}`}
                    value={volunteerQS?.Show}
                    disabled={IsLocked}
                    label="Show"
                  />
                  <LabeledCheckBox
                    setFieldValue={() =>
                      setQuestion({
                        ...question,
                        QuestionSettings: [{ ...updatedVolunteerSetting('required') }]
                      })
                    }
                    dataTestId={`requireCheckboxOfQuestion-${ID}-${index}`}
                    value={volunteerQS?.Required}
                    disabled={IsLocked}
                    label="Require"
                  />
                </Grid>
              )}
            </>
          </Grid>
          {QuestionTypeId === checkBox && (
            <CheckBoxLink
              ID={ID}
              Link={Link}
              index={index}
              errors={errors}
              touched={touched}
              question={question}
              IsLocked={IsLocked}
              setQuestion={setQuestion}
              customQuestion={customQuestion}
              QuestionDataTypeId={QuestionDataTypeId}
            />
          )}
          {QuestionTypeId === rating && (
            <RatingQuestion options={options} setOptions={setOptions} />
          )}
          {QuestionTypeId === mood && (
            <MoodQuestion
              initialized={initialized}
              setQuestion={setQuestion}
              setOptions={setOptions}
              question={question}
              touched={touched}
              options={options}
              errors={errors}
            />
          )}
          <RenderOptions
            showOptions={showOptions}
            initialized={initialized}
            options={options}
            question={question}
            setOptions={setOptions}
            setQuestion={setQuestion}
            touched={touched}
            errors={errors}
          />
        </Grid>
        <Grid container className={classes.questionIconsWrapper}>
          {IsLocked ? (
            <Box style={{ display: 'flex', justifyContent: 'flex-end' }}>
              <StyledImage src={LockIcon} data-testid={`LockIconOfQuestion-${ID}-${index}`} />
            </Box>
          ) : (
            <>
              <QuestionControlActions
                isOpen={isOpen}
                setIsOpen={setIsOpen}
                setdeleteSectionId={setdeleteSectionId}
                question={question}
                handleDeleteSection={handleDeleteSection}
                handleCloseDialog={handleCloseDialog}
                index={index}
                handleCopy={handleCopy}
                handleDelete={handleDelete}
                handleAddNewQuestion={handleAddNewQuestion}
              />
              <Box>
                <LabeledCheckBox
                  setFieldValue={() => setQuestion({ ...question, IsActive: !question?.IsActive })}
                  dataTestId={`hideCheckboxOfQuestion-${ID}-${index}`}
                  value={!question?.IsActive}
                  disabled={IsLocked}
                  label="Hide"
                />
              </Box>
            </>
          )}
        </Grid>
      </Box>
    </ClickAwayListener>
  );
};

export default RenderQuestion;
